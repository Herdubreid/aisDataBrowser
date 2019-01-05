using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celin.F9860
{
    public class Row : AIS.Row
    {
        public string F9860_OBNM { get; set; }
        public string F9860_MD { get; set; }
    }
    public class Response : AIS.FormResponse
    {
        public AIS.Form<AIS.FormData<Row>> fs_DATABROWSE_F9860;
    }
    public class Request : AIS.DatabrowserRequest
    {
        public Request()
        {

            dataServiceType = "BROWSE";
            targetName = "F9860";
            targetType = "table";
            findOnEntry = "TRUE";
            returnControlIDs = "MD|OBNM";
            maxPageSize = "10000";
            query = new AIS.Query()
            {
                condition = new List<AIS.Condition>(new AIS.Condition[]
                {
                    new AIS.Condition()
                    {
                        value = new AIS.Value []
                        {
                            new AIS.Value()
                            {
                                content = "BSVW",
                                specialValueId = "LITERAL"
                            },
                            new AIS.Value()
                            {
                                content = "TBLE",
                                specialValueId = "LITERAL"
                            }
                        },
                        controlId = "F9860.FUNO",
                        @operator = "LIST"
                    }
                })
            };
        }
    }
}
