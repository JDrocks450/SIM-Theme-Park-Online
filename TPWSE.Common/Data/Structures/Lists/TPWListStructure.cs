namespace SimTheme_Park_Online.Data.Structures
{
    public class TPWListStructure
    {
        public TPWServersideList List { get; private set; }

        private TPWListStructure()
        {

        }

        protected TPWListStructure(uint ListType, params object[] ListItems)
        {
            List = TPWServersideList.Create(ListType, ListItems);
        }

        public static TPWListStructure GenerateEmpty(uint ListType, params object[] ListItems)
        {
            var returnValue = new TPWServersideList();
            foreach(var def in ListItems)            
                returnValue.Definitions.Add(new TPWServersideList.TPWServersideListDefinition(def));
            return new TPWListStructure()
            {
                List = returnValue
            };
        }
    }
}
