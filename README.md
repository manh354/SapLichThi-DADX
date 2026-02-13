# Hệ Thống Sắp Xếp Lịch Thi (SapLichThi)

Dự án cung cấp giải pháp tự động hóa việc xếp lịch thi cho các trường đại học, giải quyết bài toán lập lịch thi (Exam Timetabling Problem) phức tạp dựa trên các thuật toán tối ưu hóa hiện đại. Hệ thống hỗ trợ xử lý các bộ dữ liệu chuẩn quốc tế như **Toronto Benchmark** và **ITC 2007**.

## 🚀 Tính Năng Nổi Bật

*   **Thuật Toán Tối Ưu**: Sử dụng **Simulated Annealing (Tôi luyện mô phỏng)** kết hợp với các kỹ thuật tìm kiếm cục bộ (Neighborhood Search) để tìm kiếm lời giải tối ưu.
*   **Xử Lý Ràng Buộc Đa Dạng**:
    *   **Ràng buộc cứng (Hard Constraints)**: Không trùng lịch thi cho sinh viên, không vượt quá sức chứa phòng.
    *   **Ràng buộc mềm (Soft Constraints)**: Tối ưu hóa khoảng cách giữa các môn thi, tránh thi liên tiếp, dồn lịch thi hợp lý.
*   **Hỗ Trợ Benchmark**: Tích hợp sẵn bộ dữ liệu và công cụ đánh giá cho:
    *   Toronto Benchmark (Carter et al.)
    *   International Timetabling Competition (ITC) 2007
*   **Kiến trúc Modular**: Thiết kế phân tách rõ ràng giữa Core, Algorithm, I/O và Runner.

## 🧠 Chi Tiết Thuật Toán & Thiết Kế

### 1. Thuật Toán Xếp Lịch (Scheduling Algorithms)
Hệ thống áp dụng quy trình 2 giai đoạn để giải quyết bài toán xếp lịch thi (Exam Timetabling Problem):

*   **Giai đoạn 1: Xây dựng lời giải ban đầu (Construction)**
    *   Sử dụng các heuristic tô màu đồ thị (**Graph Coloring**) như **Saturation Degree** (SD) hoặc **Largest Degree** (LD).
    *   Mục tiêu: Nhanh chóng tạo ra một lịch thi hợp lệ (Feasible Solution) thỏa mãn tất cả ràng buộc cứng (không trùng giờ, đủ chỗ ngồi).
    *   Kỹ thuật **Bin Packing** được sử dụng để xếp tối ưu các lớp thi vào phòng thi, giảm thiểu số lượng phòng sử dụng.

*   **Giai đoạn 2: Tối ưu hóa (Optimization)**
    *   Sử dụng giải thuật **Simulated Annealing** (Tôi luyện mô phỏng) để giảm thiểu vi phạm các ràng buộc mềm (Soft Constraints).
    *   **Cơ chế làm mát (Cooling Schedule)**: Nhiệt độ giảm dần theo công thức hình học, kiểm soát khả năng chấp nhận lời giải tồi hơn để thoát khỏi cực trị địa phương (Local Optima).
    *   **Không gian tìm kiếm (Neighborhood Search)**: Sử dụng đa dạng các toán tử di chuyển để khám phá không gian lời giải:
        *   `Move`: Chuyển 1 ca thi sang thời gian/phòng khác.
        *   `Swap`: Hoán đổi 2 ca thi cho nhau.
        *   `Kempe Chain`: Hoán đổi chuỗi các ca thi xung đột (dựa trên lý thuyết đồ thị) để tạo bước đi lớn cho việc di chuyển.

### 2. Thiết Kế Hệ Thống (System Architecture)
Dự án được thiết kế theo kiến trúc phân lớp (Layered Architecture) nhằm đảm bảo tính bảo trì và mở rộng:

*   **Domain Layer (`SapLichThiCore`)**: Chứa các thực thể cốt lõi (Core Entities) như `Student`, `ExamClass`, `Room`, `Period`. Layer này độc lập hoàn toàn với các layer khác.
*   **Algorithm Layer (`SapLichThiAlgorithm`)**:
    *   Chứa logic tính toán tối ưu.
    *   Các thành phần `Evaluator` (Bộ đánh giá) và `NeighborhoodMove` (Toán tử lân cận) được thiết kế theo mẫu **Strategy Pattern**, cho phép dễ dàng thay thế hoặc thêm mới các thuật toán mà không ảnh hưởng đến hệ thống.
*   **Infrastructure Layer (`SapLichThiStream`)**: Xử lý các tác vụ nhập xuất vật lý (đọc file Excel/CSV, ghi file log, parser cho định dạng ITC 2007).
*   **Orchestration Layer (`SapLichThiAutomatic`)**: Đóng vai trò điều phối viên, kết nối dữ liệu đầu vào với thuật toán và quản lý quy trình chạy tự động (Pipeline).

## 📂 Cấu Trúc Dự Án

Giải pháp bao gồm các project chính sau:

| Project | Mô Tả |
| :--- | :--- |
| **SapLichThiCore** | Định nghĩa các cấu trúc dữ liệu nền tảng (Student, ExamClass, Room, Period) và các mô hình bài toán. Đóng vai trò là Domain Layer. |
| **SapLichThiAlgorithm** | Thư viện lõi chứa các thuật toán xếp lịch chi tiết, bao gồm Simulated Annealing, Graph Coloring, Neighborhood Moves và Evaluators. |
| **SapLichThiNew** | Layer điều phối quy trình nghiệp vụ (Business Workflow). Định nghĩa các bước xử lý cụ thể: Tạo Context -> Xây dựng lời giải ban đầu -> Tối ưu hóa. |
| **SapLichThiAutomatic** | Wrapper tự động hóa, quản lý việc đăng ký sự kiện (Logging, Error Handling) và kích hoạt quy trình chạy. |
| **SapLichThiStream** | (Thư mục `SapLichThiFile`) Xử lý nhập/xuất dữ liệu (Đọc file Excel/CSV, Parser định dạng ITC 2007) và ghi Log hệ thống. |
| **AlgorithmExtensions** | Thư viện tiện ích chứa các hàm bổ trợ toán học, xử lý danh sách, xác suất và tập con. |
| **SapLichThiWebConsole** | Ứng dụng Console đóng vai trò là entry point để chạy các kịch bản benchmark và đánh giá kết quả, kết nối tất cả các layer trên. |

## 🛠️ Yêu Cầu Hệ Thống

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   Visual Studio 2022 hoặc VS Code.

## 🚀 Hướng Dẫn Cài Đặt & Chạy

1.  **Clone repository**:
    ```bash
    git clone https://github.com/manh354/SapLichThiWeb-DADX.git
    cd SapLichThiWeb-DADX
    ```

2.  **Khôi phục các gói phụ thuộc (Restore)**:
    ```bash
    dotnet restore
    ```

3.  **Chạy ứng dụng Benchmark (Console)**:
    Điều hướng đến thư mục `SapLichThiConsole` và chạy lệnh:
    ```bash
    cd SapLichThiConsole
    dotnet run
    ```

4.  **Sử dụng chương trình**:
    Khi chạy, chương trình sẽ hiển thị menu lựa chọn:
    *   `1`: Chạy benchmark Toronto.
    *   `2`: Chạy benchmark ITC 2007.
    *   `3`: Đánh giá các lời giải (solution) ITC 2007 đã có.

    *Lưu ý: Bạn có thể cần điều chỉnh đường dẫn file dữ liệu đầu vào trong `Program.cs` để phù hợp với máy cá nhân.*

## 📊 Đánh Giá & Kết Quả

Hệ thống cung cấp các bộ đánh giá (Evaluator) chi tiết cho từng loại bài toán:
*   **ITC 2007 Evaluator**: Tính điểm phạt dựa trên các tiêu chí: TwoInARow, TwoInADay, PeriodSpread, MixedDurations, FrontLoad, v.v.
*   **Kết quả xuất ra**: File log chi tiết và file `.sol` chứa lịch thi đã xếp.

## 🧱 Mô Tả Chi Tiết Domain Layer (SapLichThiCore)

Project **SapLichThiCore** là trái tim của hệ thống, định nghĩa các mô hình dữ liệu (Data Models) và cấu trúc cơ bản mà toàn bộ giải pháp sử dụng. Việc tách biệt này giúp các tầng khác (Algorithm, I/O) giao tiếp thống nhất thông qua các "ngôn ngữ chung".

### 1. Các Thực Thể Chính (Data Objects)
*   **Student (Sinh viên)**:
    *   `ID`: Mã số sinh viên.
    *   `Name`: Tên sinh viên (không bắt buộc đối với một số benchmark).
    *   `StudyGroupId`: Mã nhóm học (nếu có).
*   **ExamClass (Lớp thi)**:
    *   Đại diện cho một môn thi cần được xếp lịch.
    *   Chứa thông tin: `ID` (Mã lớp/môn), `Count` (Số lượng sinh viên), `Duration` (Thời gian thi), `Students` (Danh sách sinh viên tham gia).
*   **Room (Phòng thi)**:
    *   Không gian vật lý để tổ chức thi.
    *   Thuộc tính: `Capacity` (Sức chứa tối đa), `RoomType` (Small/Medium/Large), `Building` (Tòa nhà), `Floor` (Tầng).
*   **Period (Ca thi)**:
    *   Đơn vị thời gian trong lịch thi.
    *   Thuộc tính: `Index` (Chỉ số tuần tự), `Date` (Ngày thi), `Shift` (Ca trong ngày), `Duration` (Độ dài ca).

### 2. Mô Hình Ràng Buộc (Constraints)
Hệ thống hỗ trợ định nghĩa các ràng buộc phức tạp thông qua các lớp:
*   **BinaryConstraint**: Ràng buộc giữa 2 lớp thi.
    *   `SAME_SLOT`: Phải thi cùng giờ.
    *   `DIFFERENT_SLOT`: Phải thi khác giờ.
    *   `AFTER`: Lớp này phải thi sau lớp kia.
*   **UnaryConstraint**: Ràng buộc trên một lớp thi đơn lẻ (ví dụ: `ROOM_EXCLUSIVE` - phải thi phòng riêng).

### 3. Cấu Trúc Dữ Liệu Bổ Trợ (Data Structures)
*   **CustomGraph (`Graph<T>`)**:
    *   Cấu trúc đồ thị tổng quát để mô biểu diễn mối quan hệ xung đột.
    *   Sử dụng trong thuật toán tô màu đồ thị (Graph Coloring) để tìm lời giải ban đầu.
*   **DistanceArray**: (Trong `DataObjects`) Hỗ trợ tính toán khoảng cách/chi phí di chuyển giữa các phòng hoặc tòa nhà (nếu có).

## 🧠 Mô Tả Chi Tiết Algorithm Layer (SapLichThiAlgorithm)

Project **SapLichThiAlgorithm** là nơi chứa toàn bộ logic xử lý thông minh của hệ thống. Đây là nơi các thuật toán tối ưu hóa, cấu trúc dữ liệu trạng thái và bộ đánh giá được cài đặt.

### 1. Cấu Trúc Trạng Thái (State Representation - Composite Schedule)
Hệ thống sử dụng mô hình phân cấp **Composite Schedule** để biểu diễn trạng thái của lịch thi, giúp quản lý dễ dàng các thao tác di chuyển và tính toán:

*   **Lake (Hồ)**: Đại diện cho toàn bộ lịch thi (Schedule). Chứa danh sách các `Pond`.
*   **Pond (Ao)**: Đại diện cho một **Period** (Ca thi) cụ thể. Chứa danh sách các `Puddle`. Tại mỗi `Pond`, ta biết được thời gian diễn ra và danh sách các lớp thi đang diễn ra trong ca đó.
*   **Puddle (Vũng)**: Đại diện cho một **Room** (Phòng thi) trong một ca cụ thể. Đây là đơn vị nhỏ nhất chứa các `ExamClass` (Lớp thi) được xếp vào. `Puddle` quản lý sức chứa còn lại của phòng.

Cấu trúc `Lake -> Pond -> Puddle` giúp truy xuất nhanh chóng: "Tại ca X, phòng Y có những môn nào?".

### 2. Bộ Tối Ưu Hóa (Optimization Core)
*   **Simulated Annealing (`AnnealingMain`)**:
    *   Thuật toán meta-heuristic chính.
    *   Quản lý tham số nhiệt độ (`Temperature`), tốc độ làm nguội (`CoolingRate`), và độ dài chuỗi Markov (`MarkovChainLength`) để cân bằng giữa tìm kiếm rộng (Exploration) và tìm kiếm sâu (Exploitation).
    *   Sử dụng cơ chế "Patient & Disappointment" để tự động kích hoạt **Reheat** (tăng nhiệt độ lại) khi thuật toán bị kẹt ở cực trị địa phương quá lâu.

### 3. Không Gian Tìm Kiếm (Neighborhood Moves)
Các toán tử di chuyển xác định cách hệ thống chuyển từ trạng thái hiện tại sang trạng thái lân cận. Được quản lý bởi `MoveListFactory`:

*   **SingleExamClassMove**: Di chuyển một lớp thi từ phòng/ca này sang phòng/ca khác.
*   **RoomMove**: Thay đổi phòng cho một lớp thi trong cùng một ca.
*   **RoomPeriodSwapMove**: Hoán đổi vị trí của hai lớp thi bất kỳ.
*   **ShiftSwapMove**: (Toronto) Hoán đổi toàn bộ các lớp thi giữa hai ca thi.
*   **Kempe Chain Move**: Sử dụng chuỗi Kempe để hoán đổi một tập hợp các lớp thi xung đột, cho phép thực hiện các bước nhảy lớn trong không gian tìm kiếm mà không phá vỡ tính hợp lệ.

### 4. Xây Dựng Lời Giải Ban Đầu (Construction Heuristics)
*   **ClassGraphColorer**: Sử dụng thuật toán tô màu đồ thị (Graph Coloring) để tạo lịch thi ban đầu.
    *   Hỗ trợ **DSATUR** (Degree of Saturation) để chọn đỉnh khó tô nhất tô trước.
    *   Tích hợp tìm kiếm cục bộ (Local Search) ngay trong giai đoạn tô màu để giảm số màu (số ca thi) sử dụng.

### 5. Bộ Đánh Giá (Evaluators)
Được cài đặt theo **Strategy Pattern** thông qua `IEvaluator`, cho phép chuyển đổi linh hoạt cách tính điểm phạt:
*   **TorontoEvaluator**: Tính điểm dựa trên khoảng cách giữa các môn thi (Proximity Cost) theo định nghĩa của Carter et al.
*   **ITC2007Evaluator**: Tính tổng hợp nhiều loại phạt phức tạp: TwoInARow, TwoInADay, PeriodSpread, MixedDurations, FrontLoad, v.v.

## 🧱 Mô Tả Chi Tiết Infrastructure Layer (SapLichThiFile / SapLichThiStream)

Project **SapLichThiStream** (nằm trong thư mục `SapLichThiFile`) chịu trách nhiệm giao tiếp với thế giới bên ngoài, đảm bảo dữ liệu được nạp vào hệ thống một cách chuẩn hóa bất kể định dạng nguồn.

### 1. Giao Diện Chuẩn Hóa (Standard Interfaces)
Để thuật toán (Algorithm Layer) không phụ thuộc vào nguồn dữ liệu cụ thể, hệ thống định nghĩa các interface giao tiếp:
*   **`IDataSource`**: Hợp đồng cung cấp dữ liệu thô.
    *   `GetAllExamClasses()`: Lấy danh sách lớp thi.
    *   `GetRooms()`: Lấy danh sách phòng thi.
    *   `GetStudents()`: Lấy danh sách sinh viên.
*   **`ISchedulingModel`**: Hợp đồng cung cấp cấu hình bài toán và ràng buộc.
    *   `GetPeriods()`: Lấy danh sách ca thi khả dụng.
    *   `GetHardConstraints()`: Lấy cấu hình bật/tắt các ràng buộc cứng.
    *   `GetBinaryConstraints()` / `GetUnaryConstraints()`: Lấy danh sách các ràng buộc cụ thể.

### 2. Bộ Đọc Dữ Liệu (Data Readers)
Hệ thống cài đặt các bộ đọc chuyên biệt cho từng định dạng benchmark:
*   **`GeneralStreamCsvInput`**:
    *   Xử lý định dạng **Toronto Benchmark**.
    *   Đọc 2 file CSV riêng biệt: Danh sách môn học (`.crs`) và Danh sách sinh viên (`.stu`).
    *   Sử dụng thư viện `CsvHelper` để parse dữ liệu nhanh chóng và an toàn.
*   **`ITC2007ExamReader`**:
    *   Xử lý định dạng phức tạp của cuộc thi **ITC 2007**.
    *   Parser thủ công (Custom Parser) để đọc file `.exam` chứa nhiều section khác nhau (`[Exams]`, `[Periods]`, `[Rooms]`, `[Constraints]`).
    *   Đóng vai trò là cả `IDataSource` (cung cấp dữ liệu) và `ISchedulingModel` (cung cấp trọng số phạt và ràng buộc từ file input).

### 3. Xuất Dữ Liệu (Data Export)
*   **`StreamOutputExportable`**:
    *   Hỗ trợ xuất bất kỳ đối tượng nào hiện thực interface `IExportableObject` ra định dạng CSV/Excel.
    *   Được sử dụng để ghi báo cáo chi tiết hoặc xuất log debug.

## ⚙️ Mô Tả Chi Tiết Quy Trình Nghiệp Vụ (SapLichThiNew)

Project **SapLichThiNew** đóng vai trò là Business Logic Layer (hoặc Workflow Layer), nơi lắp ghép các thành phần rời rạc từ `SapLichThiAlgorithm` và `SapLichThiStream` thành một quy trình xếp lịch hoàn chỉnh.

### 1. Kiến Trúc Process-SubProcess
Project này sử dụng mẫu thiết kế **Composite Process**, trong đó mỗi bước xử lý nghiệp vụ được gói gọn trong một class kế thừa từ `AlgoProcess`:
*   **`APOverall`**: Quy trình tổng thể ("Process Manager"), chịu trách nhiệm khai báo và điều phối thứ tự chạy của các quy trình con.
*   **`AlgoProcess`**: Lớp cơ sở cung cấp cơ chế:
    *   Quản lý vòng đời: `Initialize` -> `BeforeSubprocesses` -> `RunSubprocesses` -> `AfterSubprocesses` -> `Finish`.
    *   Giao tiếp UI/Console: Thông qua các sự kiện `OnInputRequest` (hỏi người dùng tham số) và `OnConsoleLog` (xuất thông báo).

### 2. Các Bước Chính Trong Quy Trình (`APOverall`)
1.  **Context Building (`APContextBuilder`)**:
    *   Chuyển đổi dữ liệu thô từ `IDataSource` sang `AlgorithmContext` (đối tượng ngữ cảnh dùng chung cho toàn bộ thuật toán).
    *   Thiết lập các tham số ban đầu, danh sách phòng, môn thi, ràng buộc.
2.  **Preprocessing (`APPreprocessor`)**:
    *   Chuẩn hóa dữ liệu: Gom nhóm lớp học tương đương (`ClassesGrouper`), phân tích xung đột sinh viên (`CourseLinkageByCommonStudent`).
    *   Tách phòng (`RoomSeperator`): Phân loại phòng thi theo kích thước/loại hình.
3.  **Scheduling (`APTimeFixedScheduler`, `APGeneralScheduler`)**:
    *   Xếp lịch cho các môn có ràng buộc thời gian cố định trước.
    *   Xếp các môn còn lại sử dụng chiến lược **Greedy** kết hợp **Bin Packing**. (`APSmallCourseScheduler`).
4.  **Structural Building (`APStructuralBuilder`)**:
    *   Xây dựng đồ thị xung đột lớp học (`ClassGraphFiller`).
    *   Tô màu đồ thị (`ClassGraphColorer`) để tìm lời giải sơ bộ nhanh chóng.
5.  **Optimization (`APAnnealer`)**:
    *   Kích hoạt thuật toán **Simulated Annealing** để tối ưu hóa lịch thi vừa tạo.
    *   Hỏi người dùng tham số chạy (Nhiệt độ, Số vòng lặp) thông qua `APAnnealerQAndA`.
6.  **Logging (`APSettingsLogging`)**:
    *   Lưu lại các tham số cấu hình đã sử dụng ra định dạng JSON
