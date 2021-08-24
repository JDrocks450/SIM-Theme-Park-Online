namespace SimTheme_Park_Online.Data.Structures
{
    public abstract class TPWListStructure
    {
        public TPWServersideList List { get; }

        protected TPWListStructure(uint ListType, params object[] ListItems)
        {
            List = TPWServersideList.Create(ListType, ListItems);
        }
    }
}
