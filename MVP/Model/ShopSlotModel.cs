
using UnityEngine;
using UniRx;

namespace FPS.MVP
{
    public class ShopSlotModel : ModelBase
    {
        public string Name;
        public ShopData ShopData;
        public Subject<Unit> rxPurchase = new();
        public bool IsPurchased;
    }
}