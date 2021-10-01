using System.Collections.Generic;

namespace SimTheme_Park_Online.Data.Structures
{
    public class TPWListStructure
    {
        public TPWServersideList List { get; private set; }

        protected TPWListStructure()
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

        protected virtual void _fromList(TPWServersideList Packet)
        {
            List = Packet;
        }

        public static IEnumerable<T> FromPacket<T>(TPWPacket Packet) where T : TPWListStructure
        {
            var lists = TPWServersideList.Parse(Packet);
            foreach (var list in lists)
            {
                T tPWListStructure = (T) new TPWListStructure();
                tPWListStructure._fromList(list);
                yield return tPWListStructure;
            }            
        }
    }
}
