using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
       
        static void Main(string[] args)
        {
            string StrSql = @"select * from (select A.*,rownum RN  from(select a.ProcessId, a.PlateNo, a.PlateColor, a.PlateType, a.OccurTime, a.SpottingName, a.SpottingId, a.DirectionId, a.VIDEOURL, a.ASSETSNO, a.LANENO, a.ScrapreasonNo, a.USEPROPERTY, a.UPLOADFAILCOUNT, a.UPLOADSTATUS, a.RULEID,      a.DirectionName, (case when a.ISSEDIMENT = 1 then '3000' else a.Status end) as Status,a.IllegalTypeNo,a.LegalizeIllegalTypeNo,a.DepartmentId,a.PanoramaimageUrl1,a.PanoramaimageUrl2,a.AREACODE,a.RepeatCheck,
      a.PanoramaimageUrl3,a.PanoramaimageUrl4,a.ProcessingDepartment,a.VEHICLEBRAND,a.VEHICLEBRANDALIAS,a.SerialId,a.Remark,a.UPLOADPERSON,a.UPLOADTIME,a.EXPORTEDFLAG,
      a.VEHICLECOLOR,a.VEHICLETYPE,a.VEHICLESTATUS,a.OWNERNAME,a.BUSLOAD,a.LICENCEISSUINGAUTHORITY,a.ISCOMMON,a.RunSpeed,a.Checker,a.CHECKTIME,a.INPUTER,a.INPUTTIME,a.CREATEDTIME,a.SCRAPER,a.SCRAPTIME,a.UPDATETIME
      from PUNISH_ILLEGALVEHICLE a
        WHERE(A.PROCESSINGDEPARTMENT IN('1204020000', '1204030000', '1204040000', '1205000000', '1205010000', '1205020000', '1205030000', '1205040000',
        '1206000000', '1206010000', '1206020000', '1206030000', '1209000000', '1209010000', '1209020000', '1209030000', '1209040000', '1210000000', '1210010000',
        '1210020000', '1210030000', '1210040000', '1221020000', '1212010000', '1202050000', '120000001100', '1210050000', '1211000000', '1211010000', '1211020000',
        '1211030000', '1212000000', '1212020000', '1212030000', '1213000000', '1213010000', '1213020000', '1213030000', '1213040000', '1218000000', '1218010000',
        '1218020000', '1218030000', '1218040000', '1219000000', '1219010000', '1219020000', '1219030000', '0aeee8a59cf24660bea363442b9abac3', '1219040000', '1219050000',
        '1225040000', 'e06ffc19f9b74f348d96bc3edd697297', '1225060000', '1225050000', '1225070000', '1225080000', '1255000000', '1255010000', '1255020000', '1255030000',
        '1266000000', '1277000000', '1219060000', '1219070000', 'bd5dfa26a1664998ad4654bf4441eb5c', '1219080000', '1221000000', '1221010000', '1221030000', '1221040000',
        '1222000000', '1222010000', '1222020000', '1222030000', '1222040000', '1222050000', '1223000000', '1223010000', '1223020000', '1223030000', '1224000000', '1224010000',
        '1224020000', '1224030000', '1224040000', '1225000000', '1225010000', '1225020000', '1201000000', '11eb568c2f654909bcaf6b4bcb0b28e6', '1201010000', '1201020000', '1201030000',
        '1201040000', '1202000000', '1202010000', '1202020000', '1202030000', '1202040000', '1203000000', '1203010000', '1203020000', '1200000070', '1207010000', '1207020000', '1207030000',
        '1207050000', '1207060000', '1207070000', '1208000000', '1208010000', '1208020000', '1208030000', '1203030000', '1203040000', '1204000000', '1204010000') OR A.DEPARTMENTID  IN
        ('1204020000', '1204030000', '1204040000', '1205000000', '1205010000', '1205020000', '1205030000', '1205040000', '1206000000', '1206010000', '1206020000', '1206030000',
        '1209000000', '1209010000', '1209020000', '1209030000', '1209040000', '1210000000', '1210010000', '1210020000', '1210030000', '1210040000', '1221020000', '1212010000',
        '1202050000', '120000001100', '1210050000', '1211000000', '1211010000', '1211020000', '1211030000', '1212000000', '1212020000', '1212030000', '1213000000', '1213010000',
        '1213020000', '1213030000', '1213040000', '1218000000', '1218010000', '1218020000', '1218030000', '1218040000', '1219000000', '1219010000', '1219020000', '1219030000', '
        0aeee8a59cf24660bea363442b9abac3','1219040000','1219050000','1225040000','e06ffc19f9b74f348d96bc3edd697297','1225060000','1225050000','1225070000','1225080000',
        '1255000000', '1255010000', '1255020000', '1255030000', '1266000000', '1277000000', '1219060000', '1219070000', 'bd5dfa26a1664998ad4654bf4441eb5c', '1219080000', '1221000000',
        '1221010000', '1221030000', '1221040000', '1222000000', '1222010000', '1222020000', '1222030000', '1222040000', '1222050000', '1223000000', '1223010000', '1223020000', '1223030000',
        '1224000000', '1224010000', '1224020000', '1224030000', '1224040000', '1225000000', '1225010000', '1225020000', '1201000000', '11eb568c2f654909bcaf6b4bcb0b28e6', '1201010000', '1201020000',
        '1201030000', '1201040000', '1202000000', '1202010000', '1202020000', '1202030000', '1202040000', '1203000000', '1203010000', '1203020000', '1200000070', '1207010000', '1207020000', '1207030000',
        '1207050000', '1207060000', '1207070000', '1208000000', '1208010000', '1208020000', '1208030000', '1203030000', '1203040000', '1204000000', '1204010000'))   ORDER BY  OccurTime DESC ) A
                                      where  OccurTime >= to_date('2018-07-25 00:00:00', 'yyyy-mm-dd hh24:mi:ss') AND OccurTime<to_date('2018-07-28 00:00:00','yyyy-mm-dd hh24:mi:ss') and rownum <= 50
                         ) P
                         where RN > 0 order by RN ";
            string connection = string.Empty;
            string filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Config_local.config";
            StreamReader srReadFile = new StreamReader(filePath, Encoding.Default);
            while (!srReadFile.EndOfStream)  //读取流直至文件末尾结束
            {
                string strRead = srReadFile.ReadToEnd(); //读取所有数据
                connection += strRead;
            }
            srReadFile.Close(); // 关闭读取流文件
            connection = Regex.Match(connection, "(?<=value=\").*?(?=\")").Value;
            try
            {
                string conn = DESEncrypt.Decrypt(connection);
                Console.WriteLine($"链接字符串读取成功，链接地址：{conn}");
                var entity = CPQuery.From(StrSql, conn).FillDataTable();
                Console.WriteLine("datatable转化成功");
                var test = entity.ToList<IllegalVehicleReal>();
                Console.WriteLine("实体映射转化成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }
    }
}
