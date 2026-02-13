namespace SapLichThiLib.DataObjects
{
    public class Course
    {
        // Id của môn học (VD ME1202)
        string id;
        public string ID => id;
        // Phân loại môn chung / riêng
        bool isCommon = false;
        public bool IsCommon => isCommon;
        // Trường quản lý môn
        School manageSchool;
        public School School => manageSchool;
        // Trường đăng kí vào môn
        List<School> signupSchools;
        public List<School> SignupSchools => signupSchools;
        // Tên môn học
        string name;
        public string Name => name;
        // Kì học AB / 1 kì chính
        public Course(string id, bool isCommon, School manageSchool, List<School> signupSchools,string name) 
        {
            this.id = id;
            this.isCommon = isCommon;
            this.manageSchool = manageSchool;
            this.name = name;
            this.signupSchools = signupSchools;
        }

        public override string ToString()
        {
            return string.Format("Course: id: {0,-10}, isElitech: {1,-6}", id, isCommon);
        }
    }
}
