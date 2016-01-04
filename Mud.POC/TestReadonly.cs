namespace Mud.POC
{
    //http://stackoverflow.com/questions/6749022/adding-a-setter-to-a-derived-interface

    //
    public class Class1 : IReadonly1
    {
        public int Property { get; private set; }

        int IWritable1.Property
        {
            set { Property = value; }
            get { return Property; }
        }
    }

    public interface IReadonly1 : IWritable1
    {
        new int Property { get; }
    }

    public interface IWritable1
    {
        int Property { get; set; }
    }

    //
    public class Class2 : IWritable2
    {
        public int Property { get; set; }
    }

    public interface IReadonly2
    {
        int Property { get; }
    }

    public interface IWritable2 : IReadonly2
    {
        new int Property { get; set; }
    }
}
