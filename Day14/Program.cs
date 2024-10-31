int[] sonlar = [1, 2, 3, 4, 5, 6];
char[] belgilar = ['A', 'b', 'C'];

new List<int>();


sonlar.Filterlash(Juftmi).Print();
sonlar.Filterlash(x => x % 2 == 1).Print();
belgilar.Filterlash(KattaHarfmi);

bool Juftmi(int son) => son % 2 == 0;
bool KattaHarfmi(char c) => c is >= 'A' and <= 'Z';
