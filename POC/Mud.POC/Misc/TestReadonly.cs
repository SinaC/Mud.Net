namespace Mud.POC.Misc
{
    //http://stackoverflow.com/questions/6749022/adding-a-setter-to-a-derived-interface

    //
    public interface IWritable1
    {
        int Property { get; set; }
    }

    public interface IReadonly1 : IWritable1
    {
        new int Property { get; }
    }
   
    public class Class1 : IReadonly1
    {
        public int Property { get; private set; }

        int IWritable1.Property
        {
            set => Property = value;
            get => Property;
        }
    }

    public class UnitTest1 : IWritable1
    {
        public int Property { get; set; }
    }

    //
    public interface IReadonly2
    {
        int Property { get; }
    }

    public interface IWritable2 : IReadonly2
    {
        new int Property { get; set; }
    }

    public class Class2 : IWritable2
    {
        public int Property { get; set; }
    }

    public class UnitTest2 : IWritable2
    {
        public int Property { get; set; }
    }

    //
    public interface IReadonly3
    {
        int Property { get; }
    }

    public interface IWritable3
    {
        int Property { get; set; }
    }

    public class Class3 : IReadonly3, IWritable3
    {
        int IReadonly3.Property => Property;

        public int Property { get; set; }
    }

    public class UnitTest3 : IWritable3
    {
        public int Property { get; set; }
    }
}
