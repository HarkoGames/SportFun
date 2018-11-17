
namespace SportFun.Motions
{
    public enum MotionState
    {
        _UNKNOWN = 0,
        Idle = 1,
        Locomotion = 2,
        MeleeFist = 10,
        Flying = 100,
        Jump = 3,
        Climb = 4,
        Fall = 5
    }

    public enum MotionSubState
    {
        _UNKNOWN = 0,
        Attack = 1,
        Combo = 2,
        JumpingUp = 3,JumpingMidair = 4,JumpingDown = 5, Falling = 6,
        Recovery=10
    }
}
