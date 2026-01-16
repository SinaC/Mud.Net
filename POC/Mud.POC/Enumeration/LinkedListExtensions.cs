namespace Mud.POC.Enumeration
{
    public static class LinkedListExtensions
    {
        public static IEnumerable<T> SafeEnumeration<T>(this LinkedList<T> list) 
        {
            LinkedListNode<T>? nextItem;
            for (var item = list.First; item != null; item = nextItem)
            {
                nextItem = item.Next; // this will work unless nextItem is removed from the list, that will lead to stopping iteration because nextItem.Next will be set to null

                yield return item.Value;
            }
        }
    }
}
