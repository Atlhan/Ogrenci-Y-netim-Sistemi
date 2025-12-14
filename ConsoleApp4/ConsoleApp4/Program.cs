using System.Text.Json; // JSON işlemleri için
using System.Text.Json.Serialization;

// 1.1. Course (Ders) Sınıfı
public class Course
{
    public string CourseCode { get; set; }
    public string CourseName { get; set; }

    // JSON serileştirme/kullanım kolaylığı için parametresiz ctor eklenir
    public Course() { }
    public Course(string code, string name)
    {
        CourseCode = code;
        CourseName = name;
    }
}

// 1.2. Grade (Not) Sınıfı
public class Grade
{
    public string CourseCode { get; set; }
    public int Vize { get; set; }
    public int Final { get; set; }

    // Read-only (Sadece okunabilir) özellik. Değeri hesaplanarak döndürülür.
    public double Average => (Vize * 0.4) + (Final * 0.6);

    // Geçme durumu
    public string PassStatus => Average >= 60 ? "GEÇTİ" : "KALDI";

    // JSON serileştirme/kullanım kolaylığı için parametresiz ctor eklenir
    public Grade() { }
    public Grade(string courseCode, int vize, int final)
    {
        CourseCode = courseCode;
        Vize = vize;
        Final = final;
    }
}

// 1.3. Student (Öğrenci) Sınıfı
public class Student
{
    public int StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // **COLLECTION (Koleksiyon):** Öğrencinin notlarını List<T> yapısında saklar
    public List<Grade> Grades { get; set; } = new List<Grade>();

    // JSON serileştirme/kullanım kolaylığı için parametresiz ctor eklenir
    public Student() { }
    public Student(int id, string firstName, string lastName)
    {
        StudentId = id;
        FirstName = firstName;
        LastName = lastName;
    }
}

public class ManagementSystem
{
    // COLLECTION: Tüm öğrencileri List<T> içinde saklar
    private List<Student> students = new List<Student>();
    private const string FILE_NAME = "students.json";

    public ManagementSystem()
    {
        // Uygulama başladığında, eğer dosya varsa verileri yükle
        LoadDataFromJson();
    }

    // ManagementSystem.cs (Yeni Eklenecek Metotlar)

    // Yardımcı metot: Güvenli tamsayı girişi sağlar
    private int GetIntInput(string prompt)
    {
        int result;
        Console.Write(prompt);
        while (!int.TryParse(Console.ReadLine(), out result))
        {
            Console.Write("Geçersiz giriş. Lütfen bir tamsayı girin: ");
        }
        return result;
    }

    // ManagementSystem.cs içerisine eklenecek metot

    public void DisplayStudentGrades(int studentId)
    {
        // LINQ: Öğrenciyi ID'ye göre arama
        var student = students.FirstOrDefault(s => s.StudentId == studentId);

        if (student == null)
        {
            Console.WriteLine($"❌ HATA: {studentId} numaralı öğrenci bulunamadı.");
            return;
        }

        Console.WriteLine($"\n================================================");
        Console.WriteLine($"📝 {student.StudentId} - {student.FirstName} {student.LastName} Not Dökümü");
        Console.WriteLine("================================================");

        if (!student.Grades.Any()) // LINQ: Öğrencinin notu var mı kontrolü
        {
            Console.WriteLine("Öğrencinin henüz kayıtlı notu bulunmamaktadır.");
            Console.WriteLine("================================================");
            return;
        }

        // Tablo Başlığı
        Console.WriteLine("| {0,-10} | {1,-5} | {2,-5} | {3,-10} | {4,-10} |",
                          "DERS KODU", "VİZE", "FİNAL", "ORTALAMA", "DURUM");
        Console.WriteLine("|------------|-------|-------|------------|------------|");

        // Notları ortalamaya göre büyükten küçüğe sıralayarak listeleme (LINQ)
        var sortedGrades = student.Grades.OrderByDescending(g => g.Average);

        foreach (var grade in sortedGrades)
        {
            // Renk ayarlaması
            ConsoleColor statusColor = grade.PassStatus == "GEÇTİ" ? ConsoleColor.Green : ConsoleColor.Red;

            // Not satırı
            Console.Write("| {0,-10} | {1,-5} | {2,-5} |", grade.CourseCode, grade.Vize, grade.Final);

            // Ortalama ve Durum için renkli çıktı
            Console.ForegroundColor = statusColor;
            Console.Write(" {0,-10:F2} ", grade.Average);
            Console.Write("| {0,-10} |", grade.PassStatus);
            Console.ResetColor();

            Console.WriteLine();
        }

        Console.WriteLine("================================================");
    }

    // Konsol Arayüzü metodu
    public void DisplayStudentGradesFromConsole()
    {
        Console.WriteLine("\n--- Öğrenci Not Görüntüleme ---");
        int studentId = GetIntInput("Notlarını Görmek İstediğiniz Öğrenci Numarası (ID): ");
        DisplayStudentGrades(studentId);
    }
    public void DeleteStudent(int studentId)
    {
        // LINQ: ID'ye göre öğrenciyi arama
        var studentToRemove = students.FirstOrDefault(s => s.StudentId == studentId);

        if (studentToRemove != null)
        {
            students.Remove(studentToRemove); // Collection'dan kaldırma işlemi
            Console.WriteLine($"✅ BAŞARILI: {studentId} numaralı öğrenci ({studentToRemove.FirstName} {studentToRemove.LastName}) sistemden silindi.");
        }
        else
        {
            Console.WriteLine($"❌ HATA: {studentId} numaralı öğrenci bulunamadı. Silme işlemi yapılamadı.");
        }
    }

    // Konsol Arayüzü metodu
    public void DeleteStudentFromConsole()
    {
        Console.WriteLine("\n--- Öğrenci Silme ---");
        int id = GetIntInput("Silinecek Öğrenci Numarası (ID): ");
        DeleteStudent(id);
    }

    // ManagementSystem.cs içerisine eklenecek metot
public void DeleteGrade(int studentId, string courseCode)
{
    var student = students.FirstOrDefault(s => s.StudentId == studentId);
    
    if (student == null)
    {
        Console.WriteLine($"❌ HATA: {studentId} numaralı öğrenci bulunamadı.");
        return;
    }

    // LINQ: Öğrencinin notları koleksiyonu içinde ders koduna göre notu arama
    var gradeToRemove = student.Grades.FirstOrDefault(g => 
        g.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase));

    if (gradeToRemove != null)
    {
        student.Grades.Remove(gradeToRemove); // Collection'dan notu kaldırma
        Console.WriteLine($"✅ BAŞARILI: {studentId} numaralı öğrencinin {courseCode} dersine ait notu silindi.");
    }
    else
    {
        Console.WriteLine($"❌ HATA: Öğrencinin {courseCode} dersine ait notu bulunamadı.");
    }
}

// Konsol Arayüzü metodu
public void DeleteGradeFromConsole()
{
    Console.WriteLine("\n--- Not Silme ---");
    int studentId = GetIntInput("Notu Silinecek Öğrenci Numarası (ID): ");
    Console.Write("Silinecek Ders Kodu: ");
    string courseCode = Console.ReadLine();

    DeleteGrade(studentId, courseCode);
}

    // ----------------------------------------------------
    // A) Öğrenci Ekleme Fonksiyonu
    // ----------------------------------------------------
    public void AddStudentFromConsole()
    {
        Console.WriteLine("\n--- Yeni Öğrenci Bilgileri ---");
        int id = GetIntInput("Öğrenci Numarası (ID): ");

        // ID kontrolü (LINQ)
        if (students.Any(s => s.StudentId == id))
        {
            Console.WriteLine($"HATA: {id} numaralı öğrenci zaten mevcut. Lütfen benzersiz bir ID girin.");
            return;
        }

        Console.Write("Öğrenci Adı: ");
        string firstName = Console.ReadLine();
        Console.Write("Öğrenci Soyadı: ");
        string lastName = Console.ReadLine();

        // Yeni nesneyi oluştur ve listeye ekle
        AddStudent(new Student(id, firstName, lastName));
    }


    // ----------------------------------------------------
    // B) Not Ekleme Fonksiyonu
    // ----------------------------------------------------
    public void AddGradeFromConsole()
    {
        Console.WriteLine("\n--- Not Girişi ---");
        int studentId = GetIntInput("Not Eklenecek Öğrenci Numarası (ID): ");

        // Öğrenciyi bul (LINQ)
        var student = students.FirstOrDefault(s => s.StudentId == studentId);

        if (student == null)
        {
            Console.WriteLine($"HATA: {studentId} numaralı öğrenci bulunamadı.");
            return;
        }

        Console.Write("Ders Kodu (Örn: MAT101): ");
        string courseCode = Console.ReadLine();

        int vize = -1;
        while (vize < 0 || vize > 100)
        {
            vize = GetIntInput("Vize Notu (0-100): ");
            if (vize < 0 || vize > 100) Console.WriteLine("Vize notu 0 ile 100 arasında olmalıdır.");
        }

        int final = -1;
        while (final < 0 || final > 100)
        {
            final = GetIntInput("Final Notu (0-100): ");
            if (final < 0 || final > 100) Console.WriteLine("Final notu 0 ile 100 arasında olmalıdır.");
        }

        // Yeni not nesnesini oluştur ve öğrencinin Grades koleksiyonuna ekle
        AddGrade(studentId, new Grade(courseCode, vize, final));
    }

    // ----------------------------------------------------
    // C) Arama Fonksiyonu
    // ----------------------------------------------------
    public void SearchStudentFromConsole()
    {
        Console.WriteLine("\n--- Öğrenci Arama ---");
        Console.Write("Aranacak Ad veya Soyad parçası: ");
        string keyword = Console.ReadLine();

        SearchStudent(keyword); // Mevcut SearchStudent metodunu kullanır
    }

    // ----------------------------------------------------
    // KAVRAM 1: FILE OPERATIONS (JSON Yükleme/Kaydetme)
    // ----------------------------------------------------

    public void SaveDataToJson()
    {
        try
        {
            // Öğrenci listesini JSON formatına dönüştür (Serileştirme)
            var jsonString = JsonSerializer.Serialize(students, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FILE_NAME, jsonString);
            Console.WriteLine($"\n[Kayıt] Veriler {FILE_NAME} dosyasına başarıyla kaydedildi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HATA] Kayıt sırasında bir hata oluştu: {ex.Message}");
        }
    }

    public void LoadDataFromJson()
    {
        if (File.Exists(FILE_NAME))
        {
            try
            {
                var jsonString = File.ReadAllText(FILE_NAME);
                // JSON formatındaki veriyi List<Student> listesine dönüştür (Deserileştirme)
                students = JsonSerializer.Deserialize<List<Student>>(jsonString);
                Console.WriteLine($"\n[Yükleme] {students.Count} öğrenci kaydı dosyadan yüklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HATA] Veri yüklenirken bir hata oluştu: {ex.Message}");
                students = new List<Student>(); // Hata durumunda boş liste ile başla
            }
        }
    }

    // ----------------------------------------------------
    // KAVRAM 2: CRUD ve LINQ (Arama/Sıralama)
    // ----------------------------------------------------

    public void AddStudent(Student student)
    {
        if (students.Any(s => s.StudentId == student.StudentId))
        {
            Console.WriteLine($"HATA: {student.StudentId} numaralı öğrenci zaten mevcut.");
            return;
        }
        students.Add(student);
        Console.WriteLine($"Öğrenci {student.FirstName} eklendi.");
    }

    public void AddGrade(int studentId, Grade grade)
    {
        // LINQ: ID'ye göre öğrenci arama
        var student = students.FirstOrDefault(s => s.StudentId == studentId);

        if (student != null)
        {
            student.Grades.Add(grade);
            Console.WriteLine($"Not girişi yapıldı: {student.FirstName} - {grade.CourseCode}");
        }
        else
        {
            Console.WriteLine("HATA: Belirtilen öğrenci bulunamadı.");
        }
    }

    public void ListStudents(bool sort = false)
    {
        Console.WriteLine("\n--- Öğrenci Listesi ---");
        IEnumerable<Student> listToShow = students;

        if (sort)
        {
            // LINQ: Soyada göre sıralama işlemi
            listToShow = students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);
        }

        if (!listToShow.Any())
        {
            Console.WriteLine("Kayıtlı öğrenci bulunmamaktadır.");
            return;
        }

        foreach (var s in listToShow)
        {
            Console.WriteLine($"ID: {s.StudentId}, Ad Soyad: {s.FirstName} {s.LastName}");
            if (s.Grades.Count > 0)
            {
                Console.WriteLine("  Notlar:");
                // LINQ: Notları ortalamaya göre büyükten küçüğe sıralama
                var sortedGrades = s.Grades.OrderByDescending(g => g.Average);

                foreach (var g in sortedGrades)
                {
                    Console.WriteLine($"    - {g.CourseCode}: Vize={g.Vize}, Final={g.Final}, Ortalama={g.Average:F2}, Durum: {g.PassStatus}");
                }
            }
        }
    }

    public void SearchStudent(string keyword)
    {
        // LINQ: Ad veya Soyadda anahtar kelime içeren öğrencileri arama
        var results = students.Where(s =>
            s.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            s.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (results.Any())
        {
            Console.WriteLine($"\n--- Arama Sonuçları ('{keyword}') ---");
            foreach (var s in results)
            {
                Console.WriteLine($"ID: {s.StudentId}, Ad Soyad: {s.FirstName} {s.LastName}");
            }
        }
        else
        {
            Console.WriteLine($"\n'{keyword}' ile eşleşen öğrenci bulunamadı.");
        }
    }
}

// Program.cs (Giriş Noktası)

class Program
{
    static void Main(string[] args)
    {
        // Tüm iş mantığını ve veriyi yöneten nesnemiz
        ManagementSystem system = new ManagementSystem();

        bool exit = false;
        while (!exit)
        {
            DisplayMenu();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    system.AddStudentFromConsole();
                    break;
                case "2":
                    system.AddGradeFromConsole();
                    break;
                case "3":
                    system.ListStudents(sort: false);
                    break;
                case "4":
                    system.ListStudents(sort: true);
                    break;
                case "5":
                    system.DisplayStudentGradesFromConsole(); // Yeni Metot
                    break;
                case "6":
                    system.DeleteStudentFromConsole(); // Yeni Silme Metodu
                    break;
                case "7":
                    system.DeleteGradeFromConsole(); // Yeni Not Silme Metodu
                    break;
                case "8":
                    system.SaveDataToJson();
                    break;
                case "9":
                    exit = true;
                    system.SaveDataToJson();
                    break;
                default:
                    Console.WriteLine("Geçersiz seçim...");
                    break;
            }
            if (!exit)
            {
                Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                Console.ReadKey();
            }
        }
        Console.WriteLine("\nSistem kapatılıyor. Hoşça kalın.");
    }

    static void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine("🚀 Öğrenci Not Yönetim Sistemi (Console UI)");
        Console.WriteLine("==============================================");
        Console.WriteLine("1. Yeni Öğrenci Ekle");
        Console.WriteLine("2. Öğrenciye Not Girişi Yap");
        Console.WriteLine("3. Öğrencileri Listele (Sırasız)");
        Console.WriteLine("4. Öğrencileri Listele (Soyada Göre Sıralı)");
        Console.WriteLine("5. Öğrencinin Tüm Bilgilerini Görüntüle");
        Console.WriteLine("6. Öğrenci Sil"); // Yeni seçenek
        Console.WriteLine("7. Öğrenci Notu Sil"); // Yeni seçenek
        Console.WriteLine("8. Verileri JSON'a Kaydet");
        Console.WriteLine("9. Çıkış (Kaydederek Çık)");
        Console.Write("Seçiminizi yapın (1-9): "); // Seçim aralığını güncelleyin
    }
}