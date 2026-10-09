#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;

namespace Provider.Presentations
{
    internal static class WorldEventPresentations
    {
        private static readonly IReadOnlyDictionary<Type, IWorldEventPresentation> s_presentations =
            new IWorldEventPresentation[]
            {
            new Entity.EntityAppearedPresentation(),
            new Entity.FacilityAppearedPresentation(),
            new Entity.ChestAppearedPresentation(),
            new Entity.CharacterAppearedPresentation(),
            new Entity.EntityDisappearedPresentation(),
            new Entity.EntityMovedPresentation(),
            new Entity.VisibilityChangedPresentation(),
            new Entity.DirectionChangedPresentation(),
            new Entity.MaxHpChangedPresentation(),
            new Entity.ChargeStartedPresentation(),
            new Entity.ChargeCountedDownPresentation(),
            new Entity.ChargeEndedPresentation(),
            new Entity.AffiliationChangedPresentation(),
            new Entity.FacilityExhaustedPresentation(),
            new Entity.ChestLockReleasedPresentation(),
            new Entity.KeyHolderChangedPresentation(),
            new Entity.CharacterDamagedPresentation(),
            new Entity.CharacterHealedPresentation(),
            new Entity.CharacterDiedPresentation(),
            new Entity.CharacterBrokenPresentation(),
            new Entity.CharacterTurnSkippedPresentation(),
            new Combat.SkillUsedPresentation(),
            new Combat.StatueStruckPresentation(),
            new Combat.SkillFailedPresentation(),
            new Combat.SkillPartiallySucceededPresentation(),
            new Combat.ConditionInflictedPresentation(),
            new Combat.ConditionRemovedPresentation(),
            new Combat.ConditionResistedPresentation(),
            new Combat.EntityBrokenPresentation(),
            new Combat.EffectMissedPresentation(),
            new Combat.MemoryLostPresentation(),
            new Combat.ProjectileFlewPresentation(),
            new Item.ItemBrokenPresentation(),
            new Item.ItemThrownPresentation(),
            new Item.ItemDroppedPresentation(),
            new Item.ItemDiscardedPresentation(),
            new Item.ItemExchangedPresentation(),
            new Item.ItemPickedUpPresentation(),
            new Item.ItemSteppedOnPresentation(),
            new Item.ItemGivenPresentation(),
            new Item.ItemObtainedFromChestPresentation(),
            new Item.ItemsMergedPresentation(),
            new Item.MergedItemNotStoredPresentation(),
            new Item.ItemIdentifiedPresentation(),
            new Item.ItemRenamedPresentation(),
            new Item.ItemsReorderedPresentation(),
            new Item.InventoryRefreshedForDebugPresentation(),
            new Item.MoneyPickedUpPresentation(),
            new Item.ItemActionFailedPresentation(),
            new Item.FacilityItemFailedPresentation(),
            new Item.ItemChangedPresentation(),
            new Item.ItemChangeResistedPresentation(),
            new Field.MimicRevealedPresentation(),
            new Field.DeviceBrokenPresentation(),
            new Field.MonsterHouseEnteredPresentation(),
            new Field.OminousPresenceFeltPresentation(),
            new Field.TheftDetectedPresentation(),
            new Field.ShopSettledPresentation(),
            new Field.ShopPaymentRefusedPresentation(),
            new Field.FacilityUsedPresentation(),
            new Field.DebugMessagePresentation(),
            new Field.ShopEnteredPresentation(),
            new Field.ShopRoomItemsChangedPresentation(),
            new Field.ShopExitedPresentation(),
            new Map.MapEnteredPresentation(),
            new Map.TilesChangedPresentation(),
            new Map.OverlayTilesChangedPresentation(),
            new Map.TilesKnownChangedPresentation(),
            new Map.SightChangedPresentation(),
            new Map.AreaEffectAppliedPresentation(),
            new Map.GrassTrampledPresentation(),
            new Player.GameOverPresentation(),
            }.ToDictionary(presentation => presentation.EventType);

        public static IWorldEventPresentation? Find(WorldEvent worldEvent)
        {
            return s_presentations.TryGetValue(worldEvent.GetType(), out var presentation) ? presentation : null;
        }
    }
}
