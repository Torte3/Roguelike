using System;
using Domain.Model;

namespace Game
{
    public class GameInput : IInput
    {
        private Func<bool> _isDash = null!;
        private Func<bool> _isNoMove = null!;
        private Func<bool> _isDiagonalOnly = null!;

        public void Bind(Func<bool> isDash, Func<bool> isNoMove, Func<bool> isDiagonalOnly)
        {
            _isDash = isDash;
            _isNoMove = isNoMove;
            _isDiagonalOnly = isDiagonalOnly;
        }

        public bool IsDash() => _isDash();

        public bool IsNoMove() => _isNoMove();

        public bool IsDiagonalOnly() => _isDiagonalOnly();
    }
}
