#nullable enable
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class ShopInfoPresenter
    {
        [Inject]
        public ShopInfoPresenter(World world, ShopInfoView shopInfoView)
        {
            var disposables = new CompositeDisposable();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    disposables.Clear();
                    var map = mapChanged.Map;
                    if (map.Shop != null)
                    {
                        map.Shop.IsInside.SubscribeIncludingCurrentValue(isInside =>
                        {
                            shopInfoView.SetVisibility(isInside);
                        }).AddTo(disposables);
                        Observable.EveryUpdate().Where(_ => map.Shop.IsInside.CurrentValue).Subscribe(_ =>
                        {
                            Debug.Log($"PurchasePrice: {map.Shop.GetPurchasePrice(map)}, SalePrice: {map.Shop.GetSalePrice(map)}");
                            shopInfoView.SetInfo(map.Shop.GetPurchasePrice(map), map.Shop.GetSalePrice(map));
                        }).AddTo(disposables);
                    }
                    else
                    {
                        shopInfoView.SetVisibility(false);
                    }
                }
            );
        }
    }
}