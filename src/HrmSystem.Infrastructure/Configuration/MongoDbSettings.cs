namespace HrmSystem.Infrastructure.Configuration
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "HrmDb_PhamThanhHuy_2001230304";
        public string DepartmentsCollection { get; set; } = "departments";
        public string EmployeesCollection { get; set; } = "employees";
        public string ProjectsCollection { get; set; } = "projects";
        public bool AutoStartEmbeddedServerIfUnavailable { get; set; } = true;
    }
}
