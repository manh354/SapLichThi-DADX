# H? Th?ng S?p X?p L?ch Thi (SapLichThi)

D? án cung c?p gi?i pháp t? ??ng hóa vi?c x?p l?ch thi cho các tr??ng ??i h?c, gi?i quy?t bài toán l?p l?ch thi (Exam Timetabling Problem) ph?c t?p d?a trên các thu?t toán t?i ?u hóa hi?n ??i. H? th?ng h? tr? x? lý các b? d? li?u chu?n qu?c t? nh? **Toronto Benchmark** và **ITC 2007**.

## ?? Tính N?ng N?i B?t

*   **Thu?t Toán T?i ?u**: S? d?ng **Simulated Annealing (Tôi luy?n mô ph?ng)** k?t h?p v?i các k? thu?t tìm ki?m c?c b? (Neighborhood Search) ?? tìm ki?m l?i gi?i t?i ?u.
*   **X? Lý Ràng Bu?c ?a D?ng**:
    *   **Ràng bu?c c?ng (Hard Constraints)**: Không trùng l?ch thi cho sinh viên, không v??t quá s?c ch?a phòng.
    *   **Ràng bu?c m?m (Soft Constraints)**: T?i ?u hóa kho?ng cách gi?a các môn thi, tránh thi liên ti?p, d?n l?ch thi h?p lý.
*   **H? Tr? Benchmark**: Tích h?p s?n b? d? li?u và công c? ?ánh giá cho:
    *   Toronto Benchmark (Carter et al.)
    *   International Timetabling Competition (ITC) 2007
*   **Ki?n trúc Modular**: Thi?t k? phân tách rõ ràng gi?a Core, Algorithm, I/O và Runner.

## ?? Chi Ti?t Thu?t Toán & Thi?t K?

### 1. Thu?t Toán X?p L?ch (Scheduling Algorithms)
H? th?ng áp d?ng quy trình 2 giai ?o?n ?? gi?i quy?t bài toán x?p l?ch thi (Exam Timetabling Problem):

*   **Giai ?o?n 1: Xây d?ng l?i gi?i ban ??u (Construction)**
    *   S? d?ng các heuristic tô màu ?? th? (**Graph Coloring**) nh? **Saturation Degree** (SD) ho?c **Largest Degree** (LD).
    *   M?c tiêu: Nhanh chóng t?o ra m?t l?ch thi h?p l? (Feasible Solution) th?a mãn t?t c? ràng bu?c c?ng (không trùng gi?, ?? ch? ng?i).
    *   K? thu?t **Bin Packing** ???c s? d?ng ?? x?p t?i ?u các l?p thi vào phòng thi, gi?m thi?u s? l??ng phòng s? d?ng.

*   **Giai ?o?n 2: T?i ?u hóa (Optimization)**
    *   S? d?ng gi?i thu?t **Simulated Annealing** (Tôi luy?n mô ph?ng) ?? gi?m thi?u vi ph?m các ràng bu?c m?m (Soft Constraints).
    *   **C? ch? làm mát (Cooling Schedule)**: Nhi?t ?? gi?m d?n theo công th?c hình h?c, ki?m soát kh? n?ng ch?p nh?n l?i gi?i t?i h?n ?? thoát kh?i c?c tr? ??a ph??ng (Local Optima).
    *   **Không gian tìm ki?m (Neighborhood Search)**: S? d?ng ?a d?ng các toán t? di chuy?n ?? khám phá không gian l?i gi?i:
        *   `Move`: Chuy?n 1 ca thi sang th?i gian/phòng khác.
        *   `Swap`: Hoán ??i 2 ca thi cho nhau.
        *   `Kempe Chain`: Hoán ??i chu?i các ca thi xung ??t (d?a trên lý thuy?t ?? th?) ?? t?o b??c ?i l?n cho vi?c di chuy?n.

### 2. Thi?t K? H? Th?ng (System Architecture)
D? án ???c thi?t k? theo ki?n trúc phân l?p (Layered Architecture) nh?m ??m b?o tính b?o trì và m? r?ng:

*   **Domain Layer (`SapLichThiCore`)**: Ch?a các th?c th? c?t lõi (Core Entities) nh? `Student`, `ExamClass`, `Room`, `Period`. Layer này ??c l?p hoàn toàn v?i các layer khác.
*   **Algorithm Layer (`SapLichThiAlgorithm`)**:
    *   Ch?a logic tính toán t?i ?u.
    *   Các thành ph?n `Evaluator` (B? ?ánh giá) và `NeighborhoodMove` (Toán t? lân c?n) ???c thi?t k? theo m?u **Strategy Pattern**, cho phép d? dàng thay th? ho?c thêm m?i các thu?t toán mà không ?nh h??ng ??n h? th?ng.
*   **Infrastructure Layer (`SapLichThiStream`)**: X? lý các tác v? nh?p xu?t v?t lý (??c file Excel/CSV, ghi file log, parser cho ??nh d?ng ITC 2007).
*   **Orchestration Layer (`SapLichThiAutomatic`)**: ?óng vai trò ?i?u ph?i viên, k?t n?i d? li?u ??u vào v?i thu?t toán và qu?n lý quy trình ch?y t? ??ng (Pipeline).

## ?? C?u Trúc D? Án

Gi?i pháp bao g?m các project chính sau:

| Project | Mô T? |
| :--- | :--- |
| **SapLichThiCore** | ??nh ngh?a các c?u trúc d? li?u n?n t?ng (Student, ExamClass, Room, Period) và các mô hình bài toán. ?óng vai trò là Domain Layer. |
| **SapLichThiAlgorithm** | Th? vi?n lõi ch?a các thu?t toán x?p l?ch chi ti?t, bao g?m Simulated Annealing, Graph Coloring, Neighborhood Moves và Evaluators. |
| **SapLichThiNew** | Layer ?i?u ph?i quy trình nghi?p v? (Business Workflow). ??nh ngh?a các b??c x? lý c? th?: T?o Context -> Xây d?ng l?i gi?i ban ??u -> T?i ?u hóa. |
| **SapLichThiAutomatic** | Wrapper t? ??ng hóa, qu?n lý vi?c ??ng ký s? ki?n (Logging, Error Handling) và kích ho?t quy trình ch?y. |
| **SapLichThiStream** | (Th? m?c `SapLichThiFile`) X? lý nh?p/xu?t d? li?u (??c file Excel/CSV, Parser ??nh d?ng ITC 2007) và ghi Log h? th?ng. |
| **AlgorithmExtensions** | Th? vi?n ti?n ích ch?a các hàm b? tr? toán h?c, x? lý danh sách, xác su?t và t?p con. |
| **SapLichThiWebConsole** | ?ng d?ng Console ?óng vai trò là entry point ?? ch?y các k?ch b?n benchmark và ?ánh giá k?t qu?, k?t n?i t?t c? các layer trên. |

## ??? Yêu C?u H? Th?ng

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   Visual Studio 2022 ho?c VS Code.

## ?? H??ng D?n Cài ??t & Ch?y

1.  **Clone repository**:
    ```bash
    git clone https://github.com/manh354/SapLichThiWeb-DADX.git
    cd SapLichThiWeb-DADX
    ```

2.  **Khôi ph?c các gói ph? thu?c (Restore)**:
    ```bash
    dotnet restore
    ```

3.  **Ch?y ?ng d?ng Benchmark (Console)**:
    ?i?u h??ng ??n th? m?c `SapLichThiConsole` và ch?y l?nh:
    ```bash
    cd SapLichThiConsole
    dotnet run
    ```

4.  **S? d?ng ch??ng trình**:
    Khi ch?y, ch??ng trình s? hi?n th? menu l?a ch?n:
    *   `1`: Ch?y benchmark Toronto.
    *   `2`: Ch?y benchmark ITC 2007.
    *   `3`: ?ánh giá các l?i gi?i (solution) ITC 2007 ?ã có.

    *L?u ý: B?n có th? c?n ?i?u ch?nh ???ng d?n file d? li?u ??u vào trong `Program.cs` ?? phù h?p v?i máy cá nhân.*

## ?? ?ánh Giá & K?t Qu?

H? th?ng cung c?p các b? ?ánh giá (Evaluator) chi ti?t cho t?ng lo?i bài toán:
*   **ITC 2007 Evaluator**: Tính ?i?m ph?t d?a trên các tiêu chí: TwoInARow, TwoInADay, PeriodSpread, MixedDurations, FrontLoad, v.v.
*   **K?t qu? xu?t ra**: File log chi ti?t và file `.sol` ch?a l?ch thi ?ã x?p.

## ?? Mô T? Chi Ti?t Domain Layer (SapLichThiCore)

Project **SapLichThiCore** là trái tim c?a h? th?ng, ??nh ngh?a các mô hình d? li?u (Data Models) và c?u trúc c? b?n mà toàn b? gi?i pháp s? d?ng. Vi?c tách bi?t này giúp các t?ng khác (Algorithm, I/O) giao ti?p th?ng nh?t thông qua các "ngôn ng? chung".

### 1. Các Th?c Th? Chính (Data Objects)
*   **Student (Sinh viên)**:
    *   `ID`: Mã s? sinh viên.
    *   `Name`: Tên sinh viên (không b?t bu?c ??i v?i m?t s? benchmark).
    *   `StudyGroupId`: Mã nhóm h?c (n?u có).
*   **ExamClass (L?p thi)**:
    *   ??i di?n cho m?t môn thi c?n ???c x?p l?ch.
    *   Ch?a thông tin: `ID` (Mã l?p/môn), `Count` (S? l??ng sinh viên), `Duration` (Th?i gian thi), `Students` (Danh sách sinh viên tham gia).
*   **Room (Phòng thi)**:
    *   Không gian v?t lý ?? t? ch?c thi.
    *   Thu?c tính: `Capacity` (S?c ch?a t?i ?a), `RoomType` (Small/Medium/Large), `Building` (Tòa nhà), `Floor` (T?ng).
*   **Period (Ca thi)**:
    *   ??n v? th?i gian trong l?ch thi.
    *   Thu?c tính: `Index` (Ch? s? tu?n t?), `Date` (Ngày thi), `Shift` (Ca trong ngày), `Duration` (?? dài ca).

### 2. Mô Hình Ràng Bu?c (Constraints)
H? th?ng h? tr? ??nh ngh?a các ràng bu?c ph?c t?p thông qua các l?p:
*   **BinaryConstraint**: Ràng bu?c gi?a 2 l?p thi.
    *   `SAME_SLOT`: Ph?i thi cùng gi?.
    *   `DIFFERENT_SLOT`: Ph?i thi khác gi?.
    *   `AFTER`: L?p này ph?i thi sau l?p kia.
*   **UnaryConstraint**: Ràng bu?c trên m?t l?p thi ??n l? (ví d?: `ROOM_EXCLUSIVE` - ph?i thi phòng riêng).

### 3. C?u Trúc D? Li?u B? Tr? (Data Structures)
*   **CustomGraph (`Graph<T>`)**:
    *   C?u trúc ?? th? t?ng quát ?? mô bi?u di?n m?i quan h? xung ??t.
    *   S? d?ng trong thu?t toán tô màu ?? th? (Graph Coloring) ?? tìm l?i gi?i ban ??u.
*   **DistanceArray**: (Trong `DataObjects`) H? tr? tính toán kho?ng cách/chi phí di chuy?n gi?a các phòng ho?c tòa nhà (n?u có).

## ?? Mô T? Chi Ti?t Algorithm Layer (SapLichThiAlgorithm)

Project **SapLichThiAlgorithm** là n?i ch?a toàn b? logic x? lý thông minh c?a h? th?ng. ?ây là n?i các thu?t toán t?i ?u hóa, c?u trúc d? li?u tr?ng thái và b? ?ánh giá ???c cài ??t.

### 1. C?u Trúc Tr?ng Thái (State Representation - Composite Schedule)
H? th?ng s? d?ng mô hình phân c?p **Composite Schedule** ?? bi?u di?n tr?ng thái c?a l?ch thi, giúp qu?n lý d? dàng các thao tác di chuy?n và tính toán:

*   **Lake (H?)**: ??i di?n cho toàn b? l?ch thi (Schedule). Ch?a danh sách các `Pond`.
*   **Pond (Ao)**: ??i di?n cho m?t **Period** (Ca thi) c? th?. Ch?a danh sách các `Puddle`. T?i m?i `Pond`, ta bi?t ???c th?i gian di?n ra và danh sách các l?p thi ?ang di?n ra trong ca ?ó.
*   **Puddle (V?ng)**: ??i di?n cho m?t **Room** (Phòng thi) trong m?t ca c? th?. ?ây là ??n v? nh? nh?t ch?a các `ExamClass` (L?p thi) ???c x?p vào. `Puddle` qu?n lý s?c ch?a còn l?i c?a phòng.

C?u trúc `Lake -> Pond -> Puddle` giúp truy xu?t nhanh chóng: "T?i ca X, phòng Y có nh?ng môn nào?".

### 2. B? T?i ?u Hóa (Optimization Core)
*   **Simulated Annealing (`AnnealingMain`)**:
    *   Thu?t toán meta-heuristic chính.
    *   Qu?n lý tham s? nhi?t ?? (`Temperature`), t?c ?? làm ngu?i (`CoolingRate`), và ?? dài chu?i Markov (`MarkovChainLength`) ?? cân b?ng gi?a tìm ki?m r?ng (Exploration) và tìm ki?m sâu (Exploitation).
    *   S? d?ng c? ch? "Patient & Disappointment" ?? t? ??ng kích ho?t **Reheat** (t?ng nhi?t ?? l?i) khi thu?t toán b? k?t ? c?c tr? ??a ph??ng quá lâu.

### 3. Không Gian Tìm Ki?m (Neighborhood Moves)
Các toán t? di chuy?n xác ??nh cách h? th?ng chuy?n t? tr?ng thái hi?n t?i sang tr?ng thái lân c?n. ???c qu?n lý b?i `MoveListFactory`:

*   **SingleExamClassMove**: Di chuy?n m?t l?p thi t? phòng/ca này sang phòng/ca khác.
*   **RoomMove**: Thay ??i phòng cho m?t l?p thi trong cùng m?t ca.
*   **RoomPeriodSwapMove**: Hoán ??i v? trí c?a hai l?p thi b?t k?.
*   **ShiftSwapMove**: (Toronto) Hoán ??i toàn b? các l?p thi gi?a hai ca thi.
*   **Kempe Chain Move**: S? d?ng chu?i Kempe ?? hoán ??i m?t t?p h?p các l?p thi xung ??t, cho phép th?c hi?n các b??c nh?y l?n trong không gian tìm ki?m mà không phá v? tính h?p l?.

### 4. Xây D?ng L?i Gi?i Ban ??u (Construction Heuristics)
*   **ClassGraphColorer**: S? d?ng thu?t toán tô màu ?? th? (Graph Coloring) ?? t?o l?ch thi ban ??u.
    *   H? tr? **DSATUR** (Degree of Saturation) ?? ch?n ??nh khó tô nh?t tô tr??c.
    *   Tích h?p tìm ki?m c?c b? (Local Search) ngay trong giai ?o?n tô màu ?? gi?m s? màu (s? ca thi) s? d?ng.

### 5. B? ?ánh Giá (Evaluators)
???c cài ??t theo **Strategy Pattern** thông qua `IEvaluator`, cho phép chuy?n ??i linh ho?t cách tính ?i?m ph?t:
*   **TorontoEvaluator**: Tính ?i?m d?a trên kho?ng cách gi?a các môn thi (Proximity Cost) theo ??nh ngh?a c?a Carter et al.
*   **ITC2007Evaluator**: Tính t?ng h?p nhi?u lo?i ph?t ph?c t?p: TwoInARow, TwoInADay, PeriodSpread, MixedDurations, FrontLoad, v.v.

## ?? Mô T? Chi Ti?t Infrastructure Layer (SapLichThiFile / SapLichThiStream)

Project **SapLichThiStream** (n?m trong th? m?c `SapLichThiFile`) ch?u trách nhi?m giao ti?p v?i th? gi?i bên ngoài, ??m b?o d? li?u ???c n?p vào h? th?ng m?t cách chu?n hóa b?t k? ??nh d?ng ngu?n.

### 1. Giao Di?n Chu?n Hóa (Standard Interfaces)
?? thu?t toán (Algorithm Layer) không ph? thu?c vào ngu?n d? li?u c? th?, h? th?ng ??nh ngh?a các interface giao ti?p:
*   **`IDataSource`**: H?p ??ng cung c?p d? li?u thô.
    *   `GetAllExamClasses()`: L?y danh sách l?p thi.
    *   `GetRooms()`: L?y danh sách phòng thi.
    *   `GetStudents()`: L?y danh sách sinh viên.
*   **`ISchedulingModel`**: H?p ??ng cung c?p c?u hình bài toán và ràng bu?c.
    *   `GetPeriods()`: L?y danh sách ca thi kh? d?ng.
    *   `GetHardConstraints()`: L?y c?u hình b?t/t?t các ràng bu?c c?ng.
    *   `GetBinaryConstraints()` / `GetUnaryConstraints()`: L?y danh sách các ràng bu?c c? th?.

### 2. B? ??c D? Li?u (Data Readers)
H? th?ng cài ??t các b? ??c chuyên bi?t cho t?ng ??nh d?ng benchmark:
*   **`GeneralStreamCsvInput`**:
    *   X? lý ??nh d?ng **Toronto Benchmark**.
    *   ??c 2 file CSV riêng bi?t: Danh sách môn h?c (`.crs`) và Danh sách sinh viên (`.stu`).
    *   S? d?ng th? vi?n `CsvHelper` ?? parse d? li?u nhanh chóng và an toàn.
*   **`ITC2007ExamReader`**:
    *   X? lý ??nh d?ng ph?c t?p c?a cu?c thi **ITC 2007**.
    *   Parser th? công (Custom Parser) ?? ??c file `.exam` ch?a nhi?u section khác nhau (`[Exams]`, `[Periods]`, `[Rooms]`, `[Constraints]`).
    *   ?óng vai trò là c? `IDataSource` (cung c?p d? li?u) và `ISchedulingModel` (cung c?p tr?ng s? ph?t và ràng bu?c t? file input).

### 3. Xu?t D? Li?u (Data Export)
*   **`StreamOutputExportable`**:
    *   H? tr? xu?t b?t k? ??i t??ng nào hi?n th?c interface `IExportableObject` ra ??nh d?ng CSV/Excel.
    *   ???c s? d?ng ?? ghi báo cáo chi ti?t ho?c xu?t log debug.

## ?? Mô T? Chi Ti?t Quy Trình Nghi?p V? (SapLichThiNew)

Project **SapLichThiNew** ?óng vai trò là Business Logic Layer (ho?c Workflow Layer), n?i l?p ghép các thành ph?n r?i r?c t? `SapLichThiAlgorithm` và `SapLichThiStream` thành m?t quy trình x?p l?ch hoàn ch?nh.

### 1. Ki?n Trúc Process-SubProcess
Project này s? d?ng m?u thi?t k? **Composite Process**, trong ?ó m?i b??c x? lý nghi?p v? ???c gói g?n trong m?t class k? th?a t? `AlgoProcess`:
*   **`APOverall`**: Quy trình t?ng th? ("Process Manager"), ch?u trách nhi?m khai báo và ?i?u ph?i th? t? ch?y c?a các quy trình con.
*   **`AlgoProcess`**: L?p c? s? cung c?p c? ch?:
    *   Qu?n lý vòng ??i: `Initialize` -> `BeforeSubprocesses` -> `RunSubprocesses` -> `AfterSubprocesses` -> `Finish`.
    *   Giao ti?p UI/Console: Thông qua các s? ki?n `OnInputRequest` (h?i ng??i dùng tham s?) và `OnConsoleLog` (xu?t thông báo).

### 2. Các B??c Chính Trong Quy Trình (`APOverall`)
1.  **Context Building (`APContextBuilder`)**:
    *   Chuy?n ??i d? li?u thô t? `IDataSource` sang `AlgorithmContext` (??i t??ng ng? c?nh dùng chung cho toàn b? thu?t toán).
    *   Thi?t l?p các tham s? ban ??u, danh sách phòng, môn thi, ràng bu?c.
2.  **Preprocessing (`APPreprocessor`)**:
    *   Chu?n hóa d? li?u: Gom nhóm l?p h?c t??ng ???ng (`ClassesGrouper`), phân tích xung ??t sinh viên (`CourseLinkageByCommonStudent`).
    *   Tách phòng (`RoomSeperator`): Phân lo?i phòng thi theo kích th??c/lo?i hình.
3.  **Scheduling (`APTimeFixedScheduler`, `APGeneralScheduler`)**:
    *   X?p l?ch cho các môn có ràng bu?c th?i gian c? ??nh tr??c.
    *   X?p các môn còn l?i s? d?ng chi?n l??c **Greedy** k?t h?p **Bin Packing**. (`APSmallCourseScheduler`).
4.  **Structural Building (`APStructuralBuilder`)**:
    *   Xây d?ng ?? th? xung ??t l?p h?c (`ClassGraphFiller`).
    *   Tô màu ?? th? (`ClassGraphColorer`) ?? tìm l?i gi?i s? b? nhanh chóng.
5.  **Optimization (`APAnnealer`)**:
    *   Kích ho?t thu?t toán **Simulated Annealing** ?? t?i ?u hóa l?ch thi v?a t?o.
    *   H?i ng??i dùng tham s? ch?y (Nhi?t ??, S? vòng l?p) thông qua `APAnnealerQAndA`.
6.  **Logging (`APSettingsLogging`)**:
    *   L?u l?i các tham s? c?u hình ?ã s? d?ng ra ??nh d?ng JSON/XML ?? tái s? d?ng ho?c debug.

### 3. Qu?n Lý Ng? C?nh (AlgorithmContext)
`AlgorithmContext` là m?t "Global State" ch?y su?t qua pipeline x? lý:
*   ???c kh?i t?o ? b??c ??u tiên (`APContextBuilder`).
*   ???c làm giàu (enriched) d?n qua các b??c (ví d?: `APPreprocessor` thêm thông tin v? ?? th? xung ??t).
*   ???c ch?nh s?a b?i thu?t toán (ví d?: `APAnnealer` c?p nh?t l?i tr?ng thái `Lake` t?t nh?t tìm ???c).

## ?? ?óng Góp

D? án ???c phát tri?n nh?m m?c ?ích nghiên c?u và gi?i quy?t bài toán th?c t?. M?i ?óng góp (Pull Request, Issue) ??u ???c hoan nghênh.
