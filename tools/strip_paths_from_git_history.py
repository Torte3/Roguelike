#!/usr/bin/env python3
"""
Git 履歴から指定パスを除去する前にバレミラーを作成し、git-filter-repo で書き換える。

重要（既定の挙動）:
  git filter-repo 単体では「履歴から消えたパス」は現在のコミットのツリーにも存在しなくなるため、
  作業ディレクトリ上のファイルも消えます。このスクリプトの既定では、filter-repo の前に
  除去対象をリポジトリの外（--backup-dir と同じ親配下）へコピーし、成功後に元の相対パスへ
  戻します（Git には未追跡として残る）。実データを消さず履歴だけ整理する想定です。
  作業ツリーからも消したいときだけ --no-preserve-working-tree を付けてください。

前提:
  - Git が PATH にあること
  - git-filter-repo: `git filter-repo` が使えるか、同じ Python で
    `python -m git_filter_repo` が動くこと (pip install git-filter-repo)
  - 作業ツリーがクリーンであること（未コミット変更があると filter-repo が拒否することが多い）

パス:
  - STRIP_TARGET_PATHS は **正スラッシュ** で書く（Windows でも `Assets/Images/` の形。
    バックスラッシュは文字列でエスケープとして解釈されるので避ける）。

日付:
  - git-filter-repo はパス削除のような書き換えで、コミットの Author / Committer の
    日時を基本的に元のコミットから引き継ぎます（明示的に変えるオプションを付けない限り）。

使い方（リポジトリのルートで）:
  既定パスはコード内の STRIP_TARGET_PATHS を編集してから:
  python tools/strip_paths_from_git_history.py
  バックアップ先だけ変える:
  python tools/strip_paths_from_git_history.py --backup-dir D:/git-backups
  このときだけ引数でパスを上書き:
  python tools/strip_paths_from_git_history.py path1 path2
  履歴書き換え後、全ローカルブランチをリモートへ反映（省略時リモート名は origin）:
  python tools/strip_paths_from_git_history.py --push-all-to-remote
  python tools/strip_paths_from_git_history.py --push-all-to-remote mirror
  （タグも送る場合は --push-tags-after を併用。filter-repo は既定で origin を消すことが
   あるが、--push-all-to-remote 利用時は実行前に取得した URL で自動で再登録する）
  push の直前に git fetch を行い、--force-with-lease の比較先を揃える（無いと (stale info) で拒否されやすい）。
  どうしても拒否されるときだけ非推奨として --push-force を併用。
  作業ツリーからも消す（退避しない）: --no-preserve-working-tree
"""

from __future__ import annotations

import argparse
import shutil
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

# 履歴から除くパス（主にここを編集）
STRIP_TARGET_PATHS: list[str] = [
    "Assets/Plugins/Sirenix/",
    "Assets/ConsolePro/",
    "Packages/com.singularitygroup.hotreload/",
    "Assets/Aevus/",
    "Assets/HotReload",
    "Assets/Editor/WakaTime/",
    "Assets/Editor/HiArda/",
    "Assets/kyouma0220/",
    "Assets/Editor/tsubaki-wakepon/",
    ".VSCodeCounter/",
    "Assets/Animations/",
    "Assets/Images/",
    "Assets/Sounds/",
    "Assets/StateEffect/",
]


def run(
    cmd: list[str],
    *,
    cwd: Path | None = None,
    check: bool = True,
    capture_output: bool = False,
) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        cmd,
        cwd=str(cwd) if cwd else None,
        check=check,
        capture_output=capture_output,
        text=True,
        encoding="utf-8",
        errors="replace",
    )


def require_git_repo() -> Path:
    try:
        out = subprocess.check_output(
            ["git", "rev-parse", "--show-toplevel"],
            text=True,
            encoding="utf-8",
            errors="replace",
        )
    except (subprocess.CalledProcessError, FileNotFoundError) as e:
        print("エラー: Git リポジトリのルートで実行するか、リポジトリ内から起動してください。", file=sys.stderr)
        raise SystemExit(1) from e
    return Path(out.strip()).resolve()


def working_tree_clean(repo: Path) -> bool:
    r = run(
        ["git", "status", "--porcelain"],
        cwd=repo,
        check=False,
        capture_output=True,
    )
    out = (r.stdout or "").strip()
    return r.returncode == 0 and not out


def resolve_filter_repo_invocation(repo: Path) -> tuple[list[str], Path | None]:
    """
    filter-repo の起動方法を返す。

    Returns:
        (argv 先頭からサブコマンド名まで, subprocess の cwd)。
        cwd が None のときは `git -C repo` 形式なので cwd は継承でよい。
    """
    if run(["git", "filter-repo", "--version"], check=False).returncode == 0:
        return (["git", "-C", str(repo), "filter-repo"], None)
    if (
        run([sys.executable, "-m", "git_filter_repo", "--version"], check=False).returncode
        == 0
    ):
        return ([sys.executable, "-m", "git_filter_repo"], repo)
    print(
        "エラー: git-filter-repo が見つかりません。\n"
        "  pip install git-filter-repo\n"
        "のあと、次のいずれかが成功するか確認してください:\n"
        "  git filter-repo --version\n"
        f"  {sys.executable} -m git_filter_repo --version",
        file=sys.stderr,
    )
    raise SystemExit(1)


def create_mirror_backup(repo: Path, backup_dir: Path) -> Path:
    backup_dir.mkdir(parents=True, exist_ok=True)
    ts = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    name = f"{repo.name}-pre-strip-{ts}.git"
    dest = (backup_dir / name).resolve()
    if dest.exists():
        print(f"エラー: バックアップ先が既に存在します: {dest}", file=sys.stderr)
        raise SystemExit(1)
    print(f"ミラー作成: {dest}")
    run(["git", "clone", "--mirror", str(repo), str(dest)], cwd=repo.parent)
    return dest


def strip_paths(
    repo: Path,
    paths: list[str],
    *,
    force: bool,
    filter_repo: tuple[list[str], Path | None] | None = None,
) -> None:
    prefix, cwd_override = filter_repo or resolve_filter_repo_invocation(repo)
    cmd: list[str] = list(prefix)
    if force:
        cmd.append("--force")
    for p in paths:
        norm = Path(p).as_posix().strip("/")
        if ".." in Path(norm).parts:
            print(f"エラー: 不正なパス（.. を含む）: {p}", file=sys.stderr)
            raise SystemExit(1)
        cmd.extend(["--path", norm])
    cmd.append("--invert-paths")
    label = "git filter-repo" if prefix[0] == "git" else f"{sys.executable} -m git_filter_repo"
    print(f"履歴書き換え: {label}", " ".join(paths))
    run(cmd, cwd=cwd_override)


def _norm_strip_path(p: str) -> str:
    return Path(p).as_posix().strip("/")


def _paths_longest_first(paths: list[str]) -> list[str]:
    norms = sorted({_norm_strip_path(p) for p in paths}, key=lambda n: len(Path(n).parts), reverse=True)
    return norms


def preserve_working_copies(repo: Path, paths: list[str], store: Path) -> None:
    """除去対象で repo 内に存在するものを store 以下へコピー（相対パスを維持）。"""
    repo_res = repo.resolve()
    store.mkdir(parents=True, exist_ok=True)
    for norm in _paths_longest_first(paths):
        src = (repo_res / norm).resolve()
        if not src.exists():
            continue
        try:
            src.relative_to(repo_res)
        except ValueError:
            print(f"警告: リポジトリ外のため退避をスキップ: {norm}", file=sys.stderr)
            continue
        dest = (store / norm).resolve()
        dest.parent.mkdir(parents=True, exist_ok=True)
        if src.is_dir():
            if dest.exists():
                shutil.rmtree(dest)
            shutil.copytree(src, dest)
        else:
            shutil.copy2(src, dest)
        print(f"作業ツリー退避: {norm}")


def restore_working_copies(repo: Path, paths: list[str], store: Path) -> None:
    """store から repo 内の同じ相対パスへ戻す。"""
    if not store.is_dir():
        return
    repo_res = repo.resolve()
    for norm in _paths_longest_first(paths):
        src = (store / norm).resolve()
        if not src.exists():
            continue
        dest = (repo_res / norm).resolve()
        try:
            dest.relative_to(repo_res)
        except ValueError:
            continue
        dest.parent.mkdir(parents=True, exist_ok=True)
        if src.is_dir():
            if dest.exists():
                shutil.rmtree(dest)
            shutil.copytree(src, dest)
        else:
            shutil.copy2(src, dest)
        print(f"作業ツリー復元: {norm}")


def remote_exists(repo: Path, name: str) -> bool:
    r = run(
        ["git", "-C", str(repo), "remote"],
        check=False,
        capture_output=True,
    )
    if r.returncode != 0:
        return False
    names = (r.stdout or "").split()
    return name in names


def get_remote_url(repo: Path, remote: str) -> str | None:
    r = run(
        ["git", "-C", str(repo), "remote", "get-url", remote],
        check=False,
        capture_output=True,
    )
    if r.returncode != 0:
        return None
    url = (r.stdout or "").strip()
    return url or None


def restore_remote_if_missing(repo: Path, remote: str, url: str) -> None:
    if remote_exists(repo, remote):
        return
    print(
        f"git filter-repo がリモート '{remote}' を削除したため、"
        "保存していた URL で再登録します…",
    )
    run(["git", "-C", str(repo), "remote", "add", remote, url])


def fetch_remote_for_push_lease(repo: Path, remote: str) -> None:
    """
    filter-repo や remote 再登録のあと refs/remotes/<remote>/ が無いと
    git push --force-with-lease が (stale info) で全拒否になるため、先に fetch する。
    """
    print(f"fetch: {remote}（--force-with-lease 用にリモート追跡ブランチを取得）…")
    run(["git", "-C", str(repo), "fetch", remote])


def push_all_branches_to_remote(
    repo: Path,
    remote: str,
    *,
    push_tags: bool,
    force_without_lease: bool,
) -> None:
    if not remote_exists(repo, remote):
        print(
            f"エラー: リモート '{remote}' が未定義です。\n"
            "  --push-all-to-remote を使う場合は、filter-repo 実行前に "
            f"git remote add {remote} <URL> 済みである必要があります。",
            file=sys.stderr,
        )
        raise SystemExit(1)
    fetch_remote_for_push_lease(repo, remote)
    flag = "--force" if force_without_lease else "--force-with-lease"
    print(f"push: {remote} に全ブランチを {flag} で送信中…")
    run(["git", "-C", str(repo), "push", flag, remote, "--all"])
    if push_tags:
        print(f"push: {remote} にタグを {flag} で送信中…")
        run(["git", "-C", str(repo), "push", flag, remote, "--tags"])


def main() -> None:
    parser = argparse.ArgumentParser(
        description="ミラーバックアップ作成後、指定パスを Git 履歴から除去する（git-filter-repo）。",
    )
    parser.add_argument(
        "paths",
        nargs="*",
        help="省略時は STRIP_TARGET_PATHS を使用。指定時はそのリストで上書き。",
    )
    parser.add_argument(
        "--backup-dir",
        type=Path,
        default=None,
        help="バレミラーの保存ディレクトリ（既定: リポジトリの親ディレクトリ）",
    )
    parser.add_argument(
        "--no-backup",
        action="store_true",
        help="ミラーを作らずに filter-repo のみ実行する（非推奨）",
    )
    parser.add_argument(
        "--no-preserve-working-tree",
        action="store_true",
        help="退避・復元をしない（filter-repo どおり履歴と作業ツリーの両方から対象が消える）。",
    )
    parser.add_argument(
        "--force-dirty",
        action="store_true",
        help="作業ツリーに未コミット変更があっても続行する（filter-repo が拒否する場合あり）",
    )
    parser.add_argument(
        "--no-filter-repo-force",
        action="store_true",
        help="git filter-repo に --force を付けない（初回書き換え時のみ通常は不要）",
    )
    parser.add_argument(
        "--push-all-to-remote",
        nargs="?",
        const="origin",
        default=None,
        metavar="REMOTE",
        help="filter-repo 成功後、そのリモートへ全ローカルブランチを push（既定は "
        "--force-with-lease。直前に fetch する）。オプションだけなら REMOTE は origin。",
    )
    parser.add_argument(
        "--push-tags-after",
        action="store_true",
        help="--push-all-to-remote と併用。タグも同じ --force 系オプションで push。",
    )
    parser.add_argument(
        "--push-force",
        action="store_true",
        help="--push-all-to-remote 時に --force-with-lease の代わりに --force を使う（非推奨）。"
        "通常は push 直前の fetch で (stale info) は解消する。",
    )
    args = parser.parse_args()

    if args.push_force and args.push_all_to_remote is None:
        print(
            "エラー: --push-force は --push-all-to-remote と併用してください。",
            file=sys.stderr,
        )
        raise SystemExit(1)

    if args.push_tags_after and args.push_all_to_remote is None:
        print(
            "エラー: --push-tags-after は --push-all-to-remote と併用してください。",
            file=sys.stderr,
        )
        raise SystemExit(1)

    paths = list(args.paths) if args.paths else list(STRIP_TARGET_PATHS)
    if not paths:
        print(
            "エラー: 除去するパスがありません。STRIP_TARGET_PATHS に要素を追加するか、引数でパスを指定してください。",
            file=sys.stderr,
        )
        raise SystemExit(1)

    repo = require_git_repo()
    filter_repo_inv = resolve_filter_repo_invocation(repo)

    if not args.force_dirty and not working_tree_clean(repo):
        print(
            "エラー: 作業ツリーに未コミットの変更があります。\n"
            "  コミットするか stash してから再実行するか、--force-dirty を付けてください。",
            file=sys.stderr,
        )
        raise SystemExit(1)

    if shutil.which("git") is None:
        print("エラー: git が PATH にありません。", file=sys.stderr)
        raise SystemExit(1)

    push_after: tuple[str, str] | None = None
    if args.push_all_to_remote is not None:
        url = get_remote_url(repo, args.push_all_to_remote)
        if url is None:
            print(
                f"エラー: リモート '{args.push_all_to_remote}' が未定義です。\n"
                "  --push-all-to-remote を使うときは、先に "
                f"git remote add {args.push_all_to_remote} <URL> を実行してください。",
                file=sys.stderr,
            )
            raise SystemExit(1)
        push_after = (args.push_all_to_remote, url)

    backup_root = (args.backup_dir or repo.parent).resolve()
    backup_path: Path | None = None
    if not args.no_backup:
        backup_path = create_mirror_backup(repo, backup_root)
    else:
        print("警告: --no-backup のためミラーは作成しません。", file=sys.stderr)

    preserve_working = not args.no_preserve_working_tree
    preserve_store: Path | None = None
    if preserve_working:
        ts = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S.%fZ")
        preserve_store = (backup_root / f"{repo.name}-pre-strip-working-{ts}").resolve()
        if preserve_store.exists():
            print(f"エラー: 退避先が既に存在します: {preserve_store}", file=sys.stderr)
            raise SystemExit(1)
        print(
            "除去対象を作業ツリーから失わないよう、filter-repo 前に退避します…\n"
            f"  退避先: {preserve_store}",
        )
        preserve_working_copies(repo, paths, preserve_store)

    try:
        strip_paths(
            repo,
            paths,
            force=not args.no_filter_repo_force,
            filter_repo=filter_repo_inv,
        )
    except subprocess.CalledProcessError:
        print(
            "\nfilter-repo が失敗しました。"
            + (f"\nミラーは次に残っています（手元で削除可）: {backup_path}" if backup_path else "")
            + (f"\n作業ツリー退避は次に残っています: {preserve_store}" if preserve_store else ""),
            file=sys.stderr,
        )
        raise SystemExit(1)

    if preserve_store is not None:
        print("filter-repo 完了。退避から作業ツリーへ復元します…")
        restore_working_copies(repo, paths, preserve_store)
        print(
            f"復元済み（Git 上は未追跡の可能性があります）。退避のコピーは残しています: {preserve_store}\n"
            "  再びコミットしない場合は .gitignore への追加を検討してください。",
        )

    print("完了。")
    if backup_path:
        print(f"バックアップ（バレミラー）: {backup_path}")
    if push_after is not None:
        try:
            remote_name, remote_url = push_after
            restore_remote_if_missing(repo, remote_name, remote_url)
            push_all_branches_to_remote(
                repo,
                remote_name,
                push_tags=args.push_tags_after,
                force_without_lease=args.push_force,
            )
        except subprocess.CalledProcessError:
            print(
                "\ngit fetch / git push が失敗しました。\n"
                "  - Windows で「User cancelled dialog」なら、認証ダイアログを完了させてください。\n"
                "  - 手元で確認: git fetch <リモート> のあと git push --force-with-lease <リモート> --all\n"
                "  - それでも拒否されるときだけ、意図を理解したうえで --push-force を併用。",
                file=sys.stderr,
            )
            raise SystemExit(1)
        print("リモートへの push まで完了しました。")
    else:
        print(
            "リモートへ反映する場合は履歴が変わるため、"
            "各ブランチで force-with-lease が必要になることがあります。"
            " このスクリプトでは --push-all-to-remote [REMOTE] を指定すると全ブランチを送れます。"
        )


if __name__ == "__main__":
    main()
