using System;
using System.Diagnostics;
using System.Xml.Linq;
using static Lua.CodeAnalysis.Syntax.DisplayStringSyntaxVisitor;
using static System.Net.Mime.MediaTypeNames;

namespace Mud.POC.Entities
{
    public interface IEntity
    {
        string Name { get; }
    }

    public interface IItem : IEntity
    {
    }

    public interface ICharacter : IEntity
    {
    }

    public interface IRoom : IEntity
    {
    }

    public interface ILinkedList<T>
    {
        ILinkedList<T>? Previous { get; set; }
        ILinkedList<T>? Next { get; set; }
    }

    public class RoomManager
    {
        private ILinkedList<IRoom>? _rooms;
        public IEnumerable<ILinkedList<IRoom>> Rooms => _rooms.Enumerate();
        public void Add(ILinkedList<IRoom> item)
        {
            ILinkedListNodeExtensions.Add(ref _rooms, item);
        }
        public void Remove(ILinkedList<IRoom> item)
        {
            ILinkedListNodeExtensions.Remove(ref _rooms, item);
        }
    }

    public class Room : IRoom, ILinkedList<IRoom>
    {
        public string Name { get; }

        public Room(string name)
        {
            Name = name;
        }

        // previous and next in room manager
        public ILinkedList<IRoom>? Previous { get; set; }
        public ILinkedList<IRoom>? Next { get; set; }


        // content
        private ILinkedList<IItem>? _content;
        public IEnumerable<ILinkedList<IItem>> Content => _content.Enumerate();
        public void AddInRoom(ILinkedList<IItem> item)
        {
            ILinkedListNodeExtensions.Add(ref _content, item);
        }
        public void RemoveFromRoom(ILinkedList<IItem> item)
        {
            ILinkedListNodeExtensions.Remove(ref _content, item);
        }

        // people
        private ILinkedList<ICharacter>? _people;
        public IEnumerable<ILinkedList<ICharacter>> People => _people.Enumerate();
        public void Enter(ILinkedList<ICharacter> character)
        {
            ILinkedListNodeExtensions.Add(ref _people, character);
        }
        public void Leave(ILinkedList<ICharacter> character)
        {
            ILinkedListNodeExtensions.Remove(ref _people, character);
        }
    }

    public class CharacterManager
    {
        private ILinkedList<ICharacter>? _characters;
        public IEnumerable<ILinkedList<ICharacter>> Characters => _characters.Enumerate();
        public void Add(ILinkedList<ICharacter> character)
        {
            ILinkedListNodeExtensions.Add(ref _characters, character);
        }
        public void Remove(ILinkedList<ICharacter> character)
        {
            ILinkedListNodeExtensions.Remove(ref _characters, character);
        }
    }

    public class Character : ICharacter, ILinkedList<ICharacter>
    {
        public string Name { get; }

        public Character(string name)
        {
            Name = name;
        }

        // TODO: should be previous in room and next in room
        public ILinkedList<ICharacter>? Previous { get; set; }
        public ILinkedList<ICharacter>? Next { get; set; }

        // TODO: additional previous and next in character manager


        // content
        private ILinkedList<IItem>? _inventory;
        public IEnumerable<ILinkedList<IItem>> Inventory => _inventory.Enumerate();
        public void AddInInventory(ILinkedList<IItem> item)
        {
            ILinkedListNodeExtensions.Add(ref _inventory, item);
        }
        public void RemoveFromInventory(ILinkedList<IItem> item)
        {
            ILinkedListNodeExtensions.Remove(ref _inventory, item);
        }
    }

    public class ItemManager
    {
        private ILinkedList<IItem>? _items;
        public IEnumerable<ILinkedList<IItem>> Items => _items.Enumerate();
        public void AddInRoom(ILinkedList<IItem> item)
        {
            ILinkedListNodeExtensions.Add(ref _items, item);
        }
        public void RemoveFromRoom(ILinkedList<IItem> item)
        {
            ILinkedListNodeExtensions.Remove(ref _items, item);
        }
    }

    //public class Item : IItem, ILinkedList<IItem>

    public static class ILinkedListNodeExtensions
    {
        // add in the beginning of the list
        public static void Add<T>(ref ILinkedList<T>? head, ILinkedList<T> item)
        {
            if (head == null)
                head = item;
            else
            {
                item.Next = head;
                head.Previous = item;
                head = item;
            }
        }

        // remove
        public static void Remove<T>(ref ILinkedList<T>? head, ILinkedList<T> item)
        {
            Debug.Assert(head != null, "This method shouldn't be called on empty list!");
            Debug.Assert(item != null, "This method shouldn't be called on null item");
            if (head == item)
            {
                if (head.Next == null)
                {
                    head = null;
                }
                else
                {
                    head.Next.Previous = null;
                    head = head.Next;
                }
            }
            else
            {
                // we are sure to have a previous because head == item has been solved in previous if
                item.Previous!.Next = item.Next;
                if (item.Next != null)
                    item.Next.Previous = item.Previous;
            }
            // clear previous/next
            item.Previous = null;
            item.Next = null;
        }

        // enumerate while pre-reading the next node to avoid issue when modifying list during enumeration
        public static IEnumerable<ILinkedList<T>> Enumerate<T>(this ILinkedList<T>? head)
        {
            if (head == null)
                yield break;
            ILinkedList<T>? nextItem;
            for (var item = head; item != null; item = nextItem)
            {
                nextItem = item.Next;

                yield return item;
            }
        }
    }
}
