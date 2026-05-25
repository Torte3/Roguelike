using Domain.Model.Setting;
using Game;
using R3;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class DamagePresenter
    {
        private readonly CompositeDisposable _disposable = new();

        [Inject]
        public DamagePresenter(World world, DamageTextSpawner damageTextSpawner, FlushController flushController)
        {
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    var map = mapChanged.Map;
                    damageTextSpawner.DeleteAllText();
                    _disposable.Add(map.Player.Character.Status.OnDamageReceived.Subscribe(damageChanged =>
                        {
                            var damagePercentageFromMaxHp = damageChanged.Damage * 100 /
                                                            map.Player.Character.CurrentMaxHp;
                            var hpPercentageFromMaxHp = map.Player.Character.CurrentHp *
                                100 / map.Player.Character.CurrentMaxHp;
                            if (damagePercentageFromMaxHp > Settings.GlobalSettings.SignificantDamageThresholdPercentage.CurrentValue ||
                                hpPercentageFromMaxHp < Settings.GlobalSettings.LowHpThresholdPercentage.CurrentValue)
                            {
                                flushController.Flush(Settings.GlobalSettings.FlushDuration.CurrentValue);
                            }
                        }
                    ));
                    _disposable.Add(map.Characters.SubscribeIncludingCurrentObservables(
                        character => character.Status.OnDamageReceived,
                        (character, damageChanged) =>
                        {
                            if (character.Entity.IsVisible)
                            {
                                var damagePercentageFromMaxHp = damageChanged.Damage * 100 /
                                                                character.CurrentMaxHp;
                                damageTextSpawner.ShowDamage(character.Entity.CurrentPosition,
                                    damageChanged.Damage, damagePercentageFromMaxHp,
                                    Settings.GlobalSettings.DamageTextDisplayTime.CurrentValue);
                            }
                        }
                    ));
                    _disposable.Add(map.Characters.SubscribeIncludingCurrentObservables(
                        character => character.Status.OnHealReceived,
                        (character, healChanged) =>
                        {
                            if (character.Entity.IsVisible)
                            {
                                var healPercentageFromMaxHp = healChanged * 100 /
                                                              character.CurrentMaxHp;
                                damageTextSpawner.ShowHeal(character.Entity.CurrentPosition, healChanged,
                                    healPercentageFromMaxHp, Settings.GlobalSettings.DamageTextDisplayTime.CurrentValue);
                            }
                        }
                    ));
                },
                _ => _disposable.Clear());
        }
    }
}