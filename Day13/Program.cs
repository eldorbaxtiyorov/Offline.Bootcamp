
Input? input = Console.ReadLine;
var name = input();
Console.WriteLine(name);
input -= Console.ReadLine;

if(input is not null)
    input();

name = input?.Invoke() ?? "null";
input += IsmimniQaytar;

var eshmat = new Student();

eshmat.StudentPrinter = PrintTalaba;


string? PrintTalaba(Student student)
{
    return "Mana  shu student";
}

string? IsmimniQaytar()
{
    return "Wahid ustoz";
}

public class Student
{
    public delegate string? PrintStudent(Student student);
    public PrintStudent? StudentPrinter { get; set; }
}


public delegate string? Input();