using DDNSUpdater.Models;

namespace DDNSUpdater.Logic;

public class DomainComparer
{
    public static List<Domain> GetObjectsNotInList(List<Domain> list1, List<Domain> list2)
    {
        List<Domain> result = new List<Domain>();

        foreach (var item1 in list1)
        {
            bool found = false;

            foreach (var item2 in list2)
            {
                if (AreEqual(item1, item2))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                result.Add(item1);
            }
        }

        return result;
    }

    private static bool AreEqual(Domain domain1, Domain domain2)
    {
        // Compare each property for equality
        return domain1.Id == domain2.Id &&
               string.Equals(domain1.DomainString, domain2.DomainString, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(domain1.Key, domain2.Key, StringComparison.OrdinalIgnoreCase);
    }
}
