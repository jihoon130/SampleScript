using UnityEngine;

public struct EnemyDieEvent {}

public struct RoundStartEvent { public int Round; }

public struct InputEnableEvent { }
public struct InputDisableEvent { }

public struct FireEvent { }
public struct PurchaseEvent { public string name; }