using Newtonsoft.Json;

namespace Mud.Repository.Tests
{
    public static class CompareExtensions
    {
        //https://stackoverflow.com/questions/10454519/best-way-to-compare-two-complex-objects
        public static bool JsonCompare(this object obj, object another, bool compareType)
        {
            if (ReferenceEquals(obj, another)) return true;
            if ((obj == null) || (another == null)) return false;
            if (compareType && obj.GetType() != another.GetType()) return false;

            var objJson = JsonConvert.SerializeObject(obj);
            var anotherJson = JsonConvert.SerializeObject(another);

            return objJson == anotherJson;
        }
    }
}
