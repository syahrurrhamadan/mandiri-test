
namespace App.Authorization;

public static class Api
{

    public static class Users
    {
        public const string AddData = "/api/v1/users/add-data";
        public const string GetDatas = "/api/v1/users/get-datas";
        public const string GetData = "/api/v1/users/get-data";
        public const string UpdateData = "/api/v1/users/update-data";
        public const string DeleteData = "/api/v1/users/delete-data";
        public const string RoleCurrent = "/api/v1/users/role-current";
        public const string RoleAssign = "/api/v1/users/role-assign";
        public const string RoleRemove = "/api/v1/users/role-remove";
    }

    public static class Permission
    {
        public const string AddData = "/api/v1/permission/add-data";
        public const string GetDatas = "/api/v1/permission/get-datas";
        public const string GetData = "/api/v1/permission/get-data";
        public const string UpdateData = "/api/v1/permission/.update-data";
        public const string DeleteData = "/api/v1/permission/.delete-data";
        public const string Refresh = "/api/v1/permission/refresh";
        public const string Current = "/api/v1/permission/current";
        public const string Assign = "/api/v1/permission/assign";
        public const string Remove = "/api/v1/permission/remove";
        public const string RouteCurrent = "/api/v1/permission/route-current";
        public const string RouteAssign = "/api/v1/permission/route-assign";
        public const string RouteRemove = "/api/v1/permission/route-remove";
    }

    public static class Role
    {
        public const string AddData = "/api/v1/role/add-data";
        public const string GetDatas = "/api/v1/role/get-datas";
        public const string GetData = "/api/v1/role/get-data";
        public const string UpdateData = "/api/v1/role/update-data";
        public const string DeleteData = "/api/v1/role/delete-data";
        public const string Assign = "/api/v1/role/assign";
    }


    public static class Route
    {
        public const string GetData = "/api/v1/route/get-data";
        public const string GetDatas = "/api/v1/route/get-datas";
        public const string Refresh = "/api/v1/route/refresh";
    }
}
