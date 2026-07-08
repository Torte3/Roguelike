using System;

namespace Domain.Model.Dungeon.Graph
{
    /// <summary>階段（PrevMap / NextMap）専用。Teleport 系ポートとは接続できない。</summary>
    [Serializable]
    public struct StairsLink { }

    /// <summary>魔法陣（TeleportIn / TeleportOut）専用。階段ポートとは接続できない。</summary>
    [Serializable]
    public struct TeleportLink { }
}
