﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
	Left, Right
}

public enum State {
	Running, Falling, Jumping, Attacking, Bow, BowJump, Sneak
}

public enum PickUp
{
    Sword, Bow, Bomb, DoubleJump, Sneak
}

public enum SkeletonState
{
    Walking, WaitingForPlayer, Attacking
}

public enum StateSkeletonBoss
{
    Walking, Idle, Attacking, Dying
}

public static class Extensions {
	public static float ToFloat(this Direction dir) {
		return dir == Direction.Left ? -1 : 1;
	}

	public static Direction ToDirection(this float dir) {
		return dir < 0 ? Direction.Left : Direction.Right;
	}
}