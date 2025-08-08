using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static string notesDir = "notes";
    
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.Clear();

        if (!Directory.Exists(notesDir))
            Directory.CreateDirectory(notesDir);

        MainMenu();
    }

    static void MainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ГОЛОВНЕ МЕНЮ ===");
            Console.WriteLine("1. Зайти у нотатки");
            Console.WriteLine("0. Вийти з програми");

            Console.Write("Виберіть опцію: ");
            var input = Console.ReadLine();

            if (input == "1")
                NotesMenu();
            else if (input == "0")
                Environment.Exit(0);
        }
    }

    static void NotesMenu()
    {
        while (true)
        {
            Console.Clear();
            var files = GetSortedNoteFiles();

            Console.WriteLine("=== СПИСОК НОТАТОК ===");
            for (int i = 0; i < files.Count; i++)
            {
                var lines = File.ReadAllLines(files[i]);
                string title = lines.Length > 0 ? lines[0] : "Без назви";
                bool isCrossed = title.StartsWith("~");
                title = title.TrimStart('-', '~');
                Console.WriteLine($"{i + 1}. {(isCrossed ? "[✓] " : "")}{title}");
            }

            Console.WriteLine("+. Додати новий нотаток");
            Console.WriteLine("~№. Закреслити нотаток");
            Console.WriteLine("-№. Видалити нотаток");
            Console.WriteLine("0. Повернутися у головне меню");
            Console.Write("Ваш вибір: ");
            string input = Console.ReadLine();

            if (input == "0") return;
            else if (input == "+") AddNote();
            else if (input.StartsWith("~")) ToggleCrossNote(input.Substring(1), files);
            else if (input.StartsWith("-")) DeleteNote(input.Substring(1), files);
            else if (int.TryParse(input, out int index) && index >= 1 && index <= files.Count)
                OpenNote(files[index - 1]);
        }
    }

    static void AddNote()
    {
        Console.Clear();
        Console.Write("Введіть назву нового нотатка: ");
        string title = Console.ReadLine();
        var files = GetSortedNoteFiles();
        string path = Path.Combine(notesDir, $"{files.Count + 1}.txt");
        File.WriteAllText(path, "-" + title + Environment.NewLine);
    }

    static void ToggleCrossNote(string number, List<string> files)
    {
        if (int.TryParse(number, out int index) && index >= 1 && index <= files.Count)
        {
            var lines = File.ReadAllLines(files[index - 1]);
            if (lines.Length > 0)
            {
                if (lines[0].StartsWith("~"))
                    lines[0] = "-" + lines[0].Substring(1);
                else
                    lines[0] = "~" + lines[0].Substring(1);
                File.WriteAllLines(files[index - 1], lines);
            }
        }
    }

    static void DeleteNote(string number, List<string> files)
    {
        if (int.TryParse(number, out int index) && index >= 1 && index <= files.Count)
        {
            File.Delete(files[index - 1]);
            // Перейменувати всі наступні
            for (int i = index; i < files.Count; i++)
            {
                File.Move(files[i], Path.Combine(notesDir, $"{i}.txt"));
            }
        }
    }

    static void OpenNote(string filepath)
    {
        while (true)
        {
            Console.Clear();
            var lines = new List<string>(File.ReadAllLines(filepath));
            Console.WriteLine("=== НОТАТОК ===");
            for (int i = 1; i < lines.Count; i++)
            {
                string marker = lines[i].StartsWith("~") ? "[✓]" : "[ ]";
                Console.WriteLine($"{i}. {marker} {lines[i].TrimStart('-', '~')}");
            }

            Console.WriteLine("+. Додати новий рядок");
            Console.WriteLine("~№. Закреслити/розкреслити рядок");
            Console.WriteLine("-№. Видалити рядок");
            Console.WriteLine("0. Повернутися у меню нотаток");
            Console.Write("Ваш вибір: ");
            string input = Console.ReadLine();

            if (input == "0") return;
            else if (input == "+")
            {
                Console.Write("Введіть текст рядка: ");
                string text = Console.ReadLine();
                lines.Add("-" + text);
                File.WriteAllLines(filepath, lines);
            }
            else if (input.StartsWith("~") && int.TryParse(input.Substring(1), out int cross) && cross >= 1 && cross < lines.Count)
            {
                if (lines[cross].StartsWith("~"))
                    lines[cross] = "-" + lines[cross].Substring(1);
                else
                    lines[cross] = "~" + lines[cross].Substring(1);
                File.WriteAllLines(filepath, lines);
            }
            else if (input.StartsWith("-") && int.TryParse(input.Substring(1), out int del) && del >= 1 && del < lines.Count)
            {
                lines.RemoveAt(del);
                File.WriteAllLines(filepath, lines);
            }
            else if (int.TryParse(input, out int edit) && edit >= 1 && edit < lines.Count)
            {
                Console.WriteLine("Поточний текст: " + lines[edit].Substring(1));
                Console.Write("Новий текст (Esc — скасувати): ");
                string newText = ReadLineWithEsc(out bool wasEsc);
                if (!wasEsc)
                {
                    char prefix = lines[edit][0];
                    lines[edit] = prefix + newText;
                    File.WriteAllLines(filepath, lines);
                }
            }
        }
    }

    static string ReadLineWithEsc(out bool wasEsc)
    {
        wasEsc = false;
        StringBuilder sb = new StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Escape)
            {
                wasEsc = true;
                Console.WriteLine();
                return "";
            }
            else if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
                Console.Write(" ");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Write(key.KeyChar);
            }
        }
        Console.WriteLine();
        return sb.ToString();
    }

    static List<string> GetSortedNoteFiles()
    {
        var files = new List<string>(Directory.GetFiles(notesDir, "*.txt"));
        files.Sort((a, b) =>
        {
            int na = int.Parse(Path.GetFileNameWithoutExtension(a));
            int nb = int.Parse(Path.GetFileNameWithoutExtension(b));
            return na.CompareTo(nb);
        });
        return files;
    }
}