using System;
using System.Collections.Generic;

[Serializable]
public class ShopData : IGameData
{
	public string Name;
	public ShopCategory ShopCategory;
	public int Price;
}
