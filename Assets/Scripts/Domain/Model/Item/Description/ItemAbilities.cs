#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain.Model.Item.Description
{
    public record ItemAbilities(ItemAbilityKind Kind, int Limit, IReadOnlyList<string> Names)
    {
        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append($"Kind = {Kind}, Limit = {Limit}, Names = {string.Join(", ", Names)}");
            return true;
        }

        public virtual bool Equals(ItemAbilities? other)
        {
            return other is not null && Kind == other.Kind && Limit == other.Limit && Names.SequenceEqual(other.Names);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Limit, Names.Count);
        }
    }
}
