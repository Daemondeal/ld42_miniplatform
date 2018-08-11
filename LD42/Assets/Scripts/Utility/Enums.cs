using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
	Left, Right
}

public enum State {
	Running, Falling, Jumping
}

public enum PickUp
{
    Sword, Bow, Bomb, DoubleJump
}

public static class Extensions {
	public static float ToFloat(this Direction dir) {
		return dir == Direction.Left ? -1 : 1;
	}

	public static Direction ToDirection(this float dir) {
		return dir < 0 ? Direction.Left : Direction.Right;
	}
}