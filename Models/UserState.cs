namespace Helicopters_Russia.Models
{
    public enum UserState
    {
        NewUser,
        Idle,
        WaitingForDirtyData,
        WaitingForCleanData,
        DbPush,
        WorkInProgress
    }
}
