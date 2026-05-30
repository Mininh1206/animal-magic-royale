namespace AnimalMagicRoyale.Core.Data
{
    /// <summary>
    /// Static class to persist player setup selections between the Main Menu and the Game Scene.
    /// </summary>
    public static class PlayerSetupData
    {
        public static AnimalType SelectedAnimal;
        public static SkinData SelectedSkin;
        public static MapData SelectedMap;
        public static TeamMode SelectedTeamMode = TeamMode.Solo;
        
        public static void Reset()
        {
            SelectedAnimal = null;
            SelectedSkin = null;
            SelectedMap = null;
            SelectedTeamMode = TeamMode.Solo;
        }
    }
}
