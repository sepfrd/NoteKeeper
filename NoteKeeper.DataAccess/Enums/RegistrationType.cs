namespace NoteKeeper.DataAccess.Enums;

public enum RegistrationType : byte
{
    Direct = 0, // Registered directly in the app
    Google = 1 // Registered via Google OAuth
}