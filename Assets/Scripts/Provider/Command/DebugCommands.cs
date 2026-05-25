#nullable enable
using VContainer;

namespace Provider
{
    /// <summary>
    /// デバッグコマンドの統合クラス
    /// 各機能別のコマンドクラスを初期化します
    /// </summary>
    public class DebugCommands
    {
        [Inject]
        public DebugCommands(
            LogCommands logCommands,
            CharacterCommands characterCommands,
            ItemCommands itemCommands,
            SpawnCommands spawnCommands,
            MapCommands mapCommands)
        {
        }
    }
}