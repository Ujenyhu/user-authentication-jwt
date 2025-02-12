namespace userauthjwt.Helpers
{
    public class Mapper<TParent, TChild> where TParent : class
                                         where TChild : class, new()
    {
        public static void map(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {

                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }

        internal static void mapList(List<TParent> parentList, List<TChild> childList)
        {
            foreach (var parent in parentList)
            {
                var child = new TChild();
                map(parent, child);
                childList.Add(child);
            }
        }
    }
}
