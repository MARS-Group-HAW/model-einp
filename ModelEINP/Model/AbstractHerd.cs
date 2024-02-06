using System.Collections.Generic;

namespace ModelEINP.Model;

public class AbstractHerd<T> where T : AbstractAnimal
{
    public readonly List<T> Animals;    
    
    public AbstractHerd(int herdId, T leader, List<T> other)
    {
        Animals = other;
        Id = herdId;
        LeadingAnimal = leader;
    }
    
    private int Id { get; }
    public T LeadingAnimal { get; }
    
}