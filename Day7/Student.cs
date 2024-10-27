namespace Offline.Bootcamp.Day7;

public class Student
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public double Grade { get; set; }


    public override bool Equals(object? obj)
    {
        return obj is Student student && Id == student.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
