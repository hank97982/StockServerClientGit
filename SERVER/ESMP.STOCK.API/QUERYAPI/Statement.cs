using ESMP.STOCK.API.Bean;
using ESMP.STOCK.API.DTO;
using ESMP.STOCK.API.DTO.Statement;
using ESMP.STOCK.API.Utils;

namespace ESMP.STOCK.API.QUERYAPI
{
    //對帳單查詢
    public class Statement : BaseAPI
    {
        private string _connstr = "";
        public Statement(string connstr) : base(connstr)
        {
            _connstr = connstr;
        }
        public object GetQuerys(StatementDTO root)
        {
            #region
            //IEnumerable<HCMIOBean> hCMIOBean = new List<HCMIOBean>();
            //IEnumerable<TMHIOBean> tMHIOBean = new List<TMHIOBean>();
            ////IEnumerable<MSTMBBean> mSTMBBean = new List<MSTMBBean>();
            //try
            //{

            //    Dapper.SqlMapper.SetTypeMap(typeof(HCMIOBean), new ColumnAttributeTypeMapper<HCMIOBean>());

            //    string sqlCommend = @"SELECT * FROM HCMIO
            //                WHERE BHNO = @BHNO
            //                AND CSEQ = @CSEQ
            //                AND TDATE >= @Sdate
            //                AND TDATE <= @Edate";
            //    var parameters = new DynamicParameters();
            //    parameters.Add("BHNO", root.Bhno, System.Data.DbType.String);
            //    parameters.Add("CSEQ", root.Cseq, System.Data.DbType.String);
            //    parameters.Add("Sdate", root.Sdate, System.Data.DbType.String);
            //    parameters.Add("Edate", root.Edate, System.Data.DbType.String);
            //    //第三題增加股票代號查詢 StockSymbol
            //    if (root.StockSymbol != "")
            //    {
            //        sqlCommend += " AND STOCK = @STOCK";
            //        parameters.Add("STOCK", root.StockSymbol, System.Data.DbType.String);
            //    }
            //    using (var conn = new SqlConnection(_connstr))
            //        hCMIOBean = conn.Query<HCMIOBean>(sqlCommend, parameters);




            //    string sqlCommend2;
            //    Dapper.SqlMapper.SetTypeMap(typeof(TMHIOBean), new ColumnAttributeTypeMapper<TMHIOBean>());

            //    sqlCommend2 = @"SELECT * FROM TMHIO
            //                WHERE BHNO = @BHNO
            //                AND CSEQ = @CSEQ
            //                AND TDATE >= @Sdate
            //                AND TDATE <= @Edate";



            //    var parameters2 = new DynamicParameters();
            //    parameters2.Add("BHNO", root.Bhno, System.Data.DbType.String);
            //    parameters2.Add("CSEQ", root.Cseq, System.Data.DbType.String);
            //    parameters2.Add("Sdate", root.Sdate, System.Data.DbType.String);
            //    parameters2.Add("Edate", root.Edate, System.Data.DbType.String);
            //    //第三題增加股票代號查詢 StockSymbol
            //    if (root.StockSymbol != "")
            //    {
            //        sqlCommend2 += " AND STOCK = @STOCK";
            //        parameters2.Add("STOCK", root.StockSymbol, System.Data.DbType.String);
            //    }
            //    using (var conn = new SqlConnection(_connstr))
            //        tMHIOBean = conn.Query<TMHIOBean>(sqlCommend2, parameters2);



            //    //string sqlCommend3 = @"SELECT * FROM MSTMB";
            //    //var parameters3 = new DynamicParameters();
            //    //using (var conn = new SqlConnection(_connstr))
            //    //    mSTMBBean = conn.Query<MSTMBBean>(sqlCommend3, parameters3);

            //}
            //catch (Exception ex)
            //{
            //    Utils.Util.Log(ex.ToString());
            //    throw;
            //}
            //Debug.WriteLine("HCMIO Count: "+ hCMIOBean.Count()+ "TMHIO Count: "+ tMHIOBean.Count());
            #endregion

            //return QueryIntoFormatString(root, hCMIOBean, tMHIOBean);
            return QueryIntoFormatString(root, QueryCondition(QueryHCMIO(), root), QueryCondition(QueryTCNUD(), root), QueryCondition(QueryTMHIO(), root), QueryCondition(QueryTCSIO(), root));
        }

        /*
         * 摘要:
         *      取得該查尋容器項目，依照篩選條件取得結果
         * 
         * 參數:
         *      QueryItems:
         *          篩選條件的所有項目
         *      
         *      root:
         *          篩選條件的依據
         * 
         * 類型參數: 
         *      T:
         *          被帶進去的DTO
         * 傳回:
         *      回傳查詢結果   
         * 
         * 例外狀況:
         * 
         */
        private List<T> QueryCondition<T>(IEnumerable<T> QueryItems, StatementDTO root)
        {
            List<T> QueryFinal = QueryItems
                .Where(x => x.GetType().GetProperty("BHNO").GetValue(x).ToString() == root.Bhno)
                .Where(x => x.GetType().GetProperty("CSEQ").GetValue(x).ToString() == root.Cseq)
                .Where(x => Convert.ToInt32(x.GetType().GetProperty("TDATE").GetValue(x)) >= Convert.ToInt32(root.Sdate))
                .Where(x => Convert.ToInt32(x.GetType().GetProperty("TDATE").GetValue(x)) <= Convert.ToInt32(root.Edate)).ToList();
            if (root.StockSymbol != "")
            {
                QueryFinal.Where(x => x.GetType().GetProperty("STOCK").GetValue(x).ToString() == root.StockSymbol);
            }
            return QueryFinal;
        }

        private object QueryIntoFormatString(StatementDTO root, List<HCMIOBean> hcmioBean, IEnumerable<TCNUDBean> tcnudBean, IEnumerable<TMHIOBean> tmhioBean, IEnumerable<TCSIOBean> tcsioBean)
        {
            try
            {
                List<Profile> profiles = new List<Profile>();




                int datetime = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));

                BillSum bill = new BillSum();
                Accsum accsum = new Accsum();

                if (Convert.ToInt32(root.Sdate) <= datetime && Convert.ToInt32(root.Edate) >= datetime)
                {
                    //沖銷賣出
                    WriteOff wfTM = new WriteOff();
                    (List<TCNUDBean> TC, List<HCNTDBean> HCNT, List<HCNRHBean> HC, List<HCMIOBean> HCM) = wfTM.StockWriteOff(tcnudBean.ToList(), tmhioBean.ToList(), tcsioBean.ToList());
                    (List<HCMIOBean> BuysNow, List<HCMIOBean> SalesNow, List<HCMIOBean> Buys, List<HCMIOBean> Sales) = hCNTDInToStatement(HCNT, HC);

                    #region 買沖
                    //歷史交易明細
                    foreach (HCMIOBean hCMIOBeanItem in BuysNow)
                    {
                        Profile profile = new Profile();
                        profile.Bhno = hCMIOBeanItem.BHNO;
                        profile.Cseq = hCMIOBeanItem.CSEQ;
                        profile.Name = "";
                        profile.Stock = hCMIOBeanItem.STOCK;
                        profile.Stocknm = SingletonQueryProviderMSTMB.queryProvider.MSTMBQueryCNAME(hCMIOBeanItem.STOCK);
                        profile.Mdate = hCMIOBeanItem.Tdate;
                        profile.Dseq = hCMIOBeanItem.DSEQ;
                        profile.Dno = hCMIOBeanItem.DNO;
                        profile.Ttype = hCMIOBeanItem.TTYPE;
                        profile.Ttypename = TtypenameCalculate("HCMIO", hCMIOBeanItem.TTYPE, hCMIOBeanItem.BSTYPE, hCMIOBeanItem.ETYPE, hCMIOBeanItem.tTypeName);
                        profile.Bstype = hCMIOBeanItem.BSTYPE;
                        profile.Bstypename = BstypenameCalculate(hCMIOBeanItem.BSTYPE);
                        profile.Etype = hCMIOBeanItem.ETYPE;
                        profile.Mprice = hCMIOBeanItem.PRICE;
                        profile.Mqty = hCMIOBeanItem.QTY;
                        profile.Mamt = hCMIOBeanItem.AMT;
                        profile.Fee = Math.Floor(FeeCalculate(hCMIOBeanItem.QTY, hCMIOBeanItem.FEE));
                        profile.Tax = hCMIOBeanItem.TAX;
                        profile.Netamt = hCMIOBeanItem.NETAMT;
                        profiles.Add(profile);
                    }


                    var profilesLQ = from p in profiles
                                     group p by 0 into g
                                     select new
                                     {
                                         Mamt = g.Sum(g => g.Mamt),
                                         Fee = g.Sum(g => g.Fee),
                                         Tax = g.Sum(g => g.Tax),
                                         Netamt = g.Sum(g => g.Netamt),
                                         Mqty = g.Sum(g => g.Mqty)
                                     };
                    //https://dotblogs.com.tw/noncoder/2019/03/25/Lambda-GroupBy-Sum
                    var profilesMqtySumLQ = from p in profiles
                                            group p by new { p.Bstype } into g
                                            select new
                                            {
                                                Bstype = g.Key.Bstype,
                                                Mqty = g.Sum(g => g.Mqty),
                                                Mant = g.Sum(g => g.Mamt)
                                            };



                    if (profilesLQ.FirstOrDefault() == null)
                    {
                        AccsumErr accsumErr = new AccsumErr();
                        accsumErr.Errcode = "0008";
                        accsumErr.Errmsg = "查無符合設定條件之資料";
                        return accsumErr;
                    }
                    foreach (var profilesItem in profilesLQ)
                    {

                        bill.Cnbamt = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mant).ToArray()[0];
                        bill.Cnsamt = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mant).ToArray()[0];
                        bill.Cnfee = profilesItem.Fee;
                        bill.Cntax = profilesItem.Tax;
                        bill.Cnnetamt = profilesItem.Netamt;
                        bill.Bqty = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mqty).ToArray()[0];
                        bill.Sqty = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mqty).ToArray()[0];

                        accsum.Errcode = "0000";
                        accsum.Errmsg = "成功";
                        accsum.Netamt = profilesItem.Netamt;
                        accsum.Fee = profilesItem.Fee;
                        accsum.Tax = profilesItem.Tax;
                        accsum.Mqty = profilesItem.Mqty;
                        accsum.Mamt = profilesItem.Mamt;
                    }
                    accsum.Sum = bill;
                    accsum.Profile = profiles;
                    #endregion
                    #region 賣沖
                    //歷史交易明細
                    foreach (HCMIOBean hCMIOBeanItem in SalesNow)
                    {
                        Profile profile = new Profile();
                        profile.Bhno = hCMIOBeanItem.BHNO;
                        profile.Cseq = hCMIOBeanItem.CSEQ;
                        profile.Name = "";
                        profile.Stock = hCMIOBeanItem.STOCK;
                        profile.Stocknm = SingletonQueryProviderMSTMB.queryProvider.MSTMBQueryCNAME(hCMIOBeanItem.STOCK);
                        profile.Mdate = hCMIOBeanItem.Tdate;
                        profile.Dseq = hCMIOBeanItem.DSEQ;
                        profile.Dno = hCMIOBeanItem.DNO;
                        profile.Ttype = hCMIOBeanItem.TTYPE;
                        profile.Ttypename = TtypenameCalculate("HCMIO", hCMIOBeanItem.TTYPE, hCMIOBeanItem.BSTYPE, hCMIOBeanItem.ETYPE, hCMIOBeanItem.tTypeName);
                        profile.Bstype = hCMIOBeanItem.BSTYPE;
                        profile.Bstypename = BstypenameCalculate(hCMIOBeanItem.BSTYPE);
                        profile.Etype = hCMIOBeanItem.ETYPE;
                        profile.Mprice = hCMIOBeanItem.PRICE;
                        profile.Mqty = hCMIOBeanItem.QTY;
                        profile.Mamt = hCMIOBeanItem.AMT;
                        profile.Fee = Math.Floor(FeeCalculate(hCMIOBeanItem.QTY, hCMIOBeanItem.FEE));
                        profile.Tax = hCMIOBeanItem.TAX;
                        profile.Netamt = hCMIOBeanItem.NETAMT;
                        profiles.Add(profile);
                    }


                    var profilesLQ2 = from p in profiles
                                      group p by 0 into g
                                      select new
                                      {
                                          Mamt = g.Sum(g => g.Mamt),
                                          Fee = g.Sum(g => g.Fee),
                                          Tax = g.Sum(g => g.Tax),
                                          Netamt = g.Sum(g => g.Netamt),
                                          Mqty = g.Sum(g => g.Mqty)
                                      };
                    //https://dotblogs.com.tw/noncoder/2019/03/25/Lambda-GroupBy-Sum
                    var profilesMqtySumLQ2 = from p in profiles
                                             group p by new { p.Bstype } into g
                                             select new
                                             {
                                                 Bstype = g.Key.Bstype,
                                                 Mqty = g.Sum(g => g.Mqty),
                                                 Mant = g.Sum(g => g.Mamt)
                                             };



                    if (profilesLQ.FirstOrDefault() == null)
                    {
                        AccsumErr accsumErr = new AccsumErr();
                        accsumErr.Errcode = "0008";
                        accsumErr.Errmsg = "查無符合設定條件之資料";
                        return accsumErr;
                    }
                    foreach (var profilesItem in profilesLQ)
                    {

                        bill.Cnbamt = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mant).ToArray()[0];
                        bill.Cnsamt = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mant).ToArray()[0];
                        bill.Cnfee = profilesItem.Fee;
                        bill.Cntax = profilesItem.Tax;
                        bill.Cnnetamt = profilesItem.Netamt;
                        bill.Bqty = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mqty).ToArray()[0];
                        bill.Sqty = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mqty).ToArray()[0];

                        accsum.Errcode = "0000";
                        accsum.Errmsg = "成功";
                        accsum.Netamt = profilesItem.Netamt;
                        accsum.Fee = profilesItem.Fee;
                        accsum.Tax = profilesItem.Tax;
                        accsum.Mqty = profilesItem.Mqty;
                        accsum.Mamt = profilesItem.Mamt;
                    }
                    accsum.Sum = bill;
                    accsum.Profile = profiles;
                    #endregion
                    #region 現買
                    //歷史交易明細
                    foreach (HCMIOBean hCMIOBeanItem in Buys)
                    {
                        Profile profile = new Profile();
                        profile.Bhno = hCMIOBeanItem.BHNO;
                        profile.Cseq = hCMIOBeanItem.CSEQ;
                        profile.Name = "";
                        profile.Stock = hCMIOBeanItem.STOCK;
                        profile.Stocknm = SingletonQueryProviderMSTMB.queryProvider.MSTMBQueryCNAME(hCMIOBeanItem.STOCK);
                        profile.Mdate = hCMIOBeanItem.Tdate;
                        profile.Dseq = hCMIOBeanItem.DSEQ;
                        profile.Dno = hCMIOBeanItem.DNO;
                        profile.Ttype = hCMIOBeanItem.TTYPE;
                        profile.Ttypename = TtypenameCalculate("HCMIO", hCMIOBeanItem.TTYPE, hCMIOBeanItem.BSTYPE, hCMIOBeanItem.ETYPE, hCMIOBeanItem.tTypeName);
                        profile.Bstype = hCMIOBeanItem.BSTYPE;
                        profile.Bstypename = BstypenameCalculate(hCMIOBeanItem.BSTYPE);
                        profile.Etype = hCMIOBeanItem.ETYPE;
                        profile.Mprice = hCMIOBeanItem.PRICE;
                        profile.Mqty = hCMIOBeanItem.QTY;
                        profile.Mamt = hCMIOBeanItem.AMT;
                        profile.Fee = Math.Floor(FeeCalculate(hCMIOBeanItem.QTY, hCMIOBeanItem.FEE));
                        profile.Tax = hCMIOBeanItem.TAX;
                        profile.Netamt = hCMIOBeanItem.NETAMT;
                        profiles.Add(profile);
                    }


                    var profilesLQ3 = from p in profiles
                                      group p by 0 into g
                                      select new
                                      {
                                          Mamt = g.Sum(g => g.Mamt),
                                          Fee = g.Sum(g => g.Fee),
                                          Tax = g.Sum(g => g.Tax),
                                          Netamt = g.Sum(g => g.Netamt),
                                          Mqty = g.Sum(g => g.Mqty)
                                      };
                    //https://dotblogs.com.tw/noncoder/2019/03/25/Lambda-GroupBy-Sum
                    var profilesMqtySumLQ3 = from p in profiles
                                             group p by new { p.Bstype } into g
                                             select new
                                             {
                                                 Bstype = g.Key.Bstype,
                                                 Mqty = g.Sum(g => g.Mqty),
                                                 Mant = g.Sum(g => g.Mamt)
                                             };


                    if (profilesLQ.FirstOrDefault() == null)
                    {
                        AccsumErr accsumErr = new AccsumErr();
                        accsumErr.Errcode = "0008";
                        accsumErr.Errmsg = "查無符合設定條件之資料";
                        return accsumErr;
                    }
                    foreach (var profilesItem in profilesLQ)
                    {

                        bill.Cnbamt = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mant).ToArray()[0];
                        bill.Cnsamt = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mant).ToArray()[0];
                        bill.Cnfee = profilesItem.Fee;
                        bill.Cntax = profilesItem.Tax;
                        bill.Cnnetamt = profilesItem.Netamt;
                        bill.Bqty = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mqty).ToArray()[0];
                        bill.Sqty = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mqty).ToArray()[0];

                        accsum.Errcode = "0000";
                        accsum.Errmsg = "成功";
                        accsum.Netamt = profilesItem.Netamt;
                        accsum.Fee = profilesItem.Fee;
                        accsum.Tax = profilesItem.Tax;
                        accsum.Mqty = profilesItem.Mqty;
                        accsum.Mamt = profilesItem.Mamt;
                    }
                    accsum.Sum = bill;
                    accsum.Profile = profiles;
                    #endregion
                    #region 先賣
                    //歷史交易明細
                    foreach (HCMIOBean hCMIOBeanItem in Sales)
                    {
                        Profile profile = new Profile();
                        profile.Bhno = hCMIOBeanItem.BHNO;
                        profile.Cseq = hCMIOBeanItem.CSEQ;
                        profile.Name = "";
                        profile.Stock = hCMIOBeanItem.STOCK;
                        profile.Stocknm = SingletonQueryProviderMSTMB.queryProvider.MSTMBQueryCNAME(hCMIOBeanItem.STOCK);
                        profile.Mdate = hCMIOBeanItem.Tdate;
                        profile.Dseq = hCMIOBeanItem.DSEQ;
                        profile.Dno = hCMIOBeanItem.DNO;
                        profile.Ttype = hCMIOBeanItem.TTYPE;
                        profile.Ttypename = TtypenameCalculate("HCMIO", hCMIOBeanItem.TTYPE, hCMIOBeanItem.BSTYPE, hCMIOBeanItem.ETYPE, hCMIOBeanItem.tTypeName);
                        profile.Bstype = hCMIOBeanItem.BSTYPE;
                        profile.Bstypename = BstypenameCalculate(hCMIOBeanItem.BSTYPE);
                        profile.Etype = hCMIOBeanItem.ETYPE;
                        profile.Mprice = hCMIOBeanItem.PRICE;
                        profile.Mqty = hCMIOBeanItem.QTY;
                        profile.Mamt = hCMIOBeanItem.AMT;
                        profile.Fee = Math.Floor(FeeCalculate(hCMIOBeanItem.QTY, hCMIOBeanItem.FEE));
                        profile.Tax = hCMIOBeanItem.TAX;
                        profile.Netamt = hCMIOBeanItem.NETAMT;
                        profiles.Add(profile);
                    }


                    var profilesLQ4 = from p in profiles
                                      group p by 0 into g
                                      select new
                                      {
                                          Mamt = g.Sum(g => g.Mamt),
                                          Fee = g.Sum(g => g.Fee),
                                          Tax = g.Sum(g => g.Tax),
                                          Netamt = g.Sum(g => g.Netamt),
                                          Mqty = g.Sum(g => g.Mqty)
                                      };
                    //https://dotblogs.com.tw/noncoder/2019/03/25/Lambda-GroupBy-Sum
                    var profilesMqtySumLQ4 = from p in profiles
                                             group p by new { p.Bstype } into g
                                             select new
                                             {
                                                 Bstype = g.Key.Bstype,
                                                 Mqty = g.Sum(g => g.Mqty),
                                                 Mant = g.Sum(g => g.Mamt)
                                             };


                    if (profilesLQ.FirstOrDefault() == null)
                    {
                        AccsumErr accsumErr = new AccsumErr();
                        accsumErr.Errcode = "0008";
                        accsumErr.Errmsg = "查無符合設定條件之資料";
                        return accsumErr;
                    }
                    foreach (var profilesItem in profilesLQ)
                    {

                        bill.Cnbamt = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mant).ToArray()[0];
                        bill.Cnsamt = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mant).ToArray()[0];
                        bill.Cnfee = profilesItem.Fee;
                        bill.Cntax = profilesItem.Tax;
                        bill.Cnnetamt = profilesItem.Netamt;
                        bill.Bqty = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mqty).ToArray()[0];
                        bill.Sqty = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mqty).ToArray()[0];

                        accsum.Errcode = "0000";
                        accsum.Errmsg = "成功";
                        accsum.Netamt = profilesItem.Netamt;
                        accsum.Fee = profilesItem.Fee;
                        accsum.Tax = profilesItem.Tax;
                        accsum.Mqty = profilesItem.Mqty;
                        accsum.Mamt = profilesItem.Mamt;
                    }
                    accsum.Sum = bill;
                    accsum.Profile = profiles;
                    #endregion
                    #region 不能被零股沖銷的賣單
                    //歷史交易明細
                    foreach (HCMIOBean hCMIOBeanItem in HCM)
                    {
                        Profile profile = new Profile();
                        profile.Bhno = hCMIOBeanItem.BHNO;
                        profile.Cseq = hCMIOBeanItem.CSEQ;
                        profile.Name = "";
                        profile.Stock = hCMIOBeanItem.STOCK;
                        profile.Stocknm = SingletonQueryProviderMSTMB.queryProvider.MSTMBQueryCNAME(hCMIOBeanItem.STOCK);
                        profile.Mdate = hCMIOBeanItem.Tdate;
                        profile.Dseq = hCMIOBeanItem.DSEQ;
                        profile.Dno = hCMIOBeanItem.DNO;
                        profile.Ttype = hCMIOBeanItem.TTYPE;
                        profile.Ttypename = TtypenameCalculate("HCMIO", hCMIOBeanItem.TTYPE, hCMIOBeanItem.BSTYPE, hCMIOBeanItem.ETYPE, hCMIOBeanItem.tTypeName);
                        profile.Bstype = hCMIOBeanItem.BSTYPE;
                        profile.Bstypename = BstypenameCalculate(hCMIOBeanItem.BSTYPE);
                        profile.Etype = hCMIOBeanItem.ETYPE;
                        profile.Mprice = hCMIOBeanItem.PRICE;
                        profile.Mqty = hCMIOBeanItem.QTY;
                        profile.Mamt = hCMIOBeanItem.AMT;
                        profile.Fee = Math.Floor(FeeCalculate(hCMIOBeanItem.QTY, hCMIOBeanItem.FEE));
                        profile.Tax = hCMIOBeanItem.TAX;
                        profile.Netamt = hCMIOBeanItem.NETAMT;
                        profiles.Add(profile);
                    }


                    var profilesLQ5 = from p in profiles
                                      group p by 0 into g
                                      select new
                                      {
                                          Mamt = g.Sum(g => g.Mamt),
                                          Fee = g.Sum(g => g.Fee),
                                          Tax = g.Sum(g => g.Tax),
                                          Netamt = g.Sum(g => g.Netamt),
                                          Mqty = g.Sum(g => g.Mqty)
                                      };
                    //https://dotblogs.com.tw/noncoder/2019/03/25/Lambda-GroupBy-Sum
                    var profilesMqtySumLQ5 = from p in profiles
                                             group p by new { p.Bstype } into g
                                             select new
                                             {
                                                 Bstype = g.Key.Bstype,
                                                 Mqty = g.Sum(g => g.Mqty),
                                                 Mant = g.Sum(g => g.Mamt)
                                             };


                    if (profilesLQ.FirstOrDefault() == null)
                    {
                        AccsumErr accsumErr = new AccsumErr();
                        accsumErr.Errcode = "0008";
                        accsumErr.Errmsg = "查無符合設定條件之資料";
                        return accsumErr;
                    }
                    foreach (var profilesItem in profilesLQ)
                    {

                        bill.Cnbamt = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mant).ToArray()[0];
                        bill.Cnsamt = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mant).ToArray()[0];
                        bill.Cnfee = profilesItem.Fee;
                        bill.Cntax = profilesItem.Tax;
                        bill.Cnnetamt = profilesItem.Netamt;
                        bill.Bqty = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mqty).ToArray()[0];
                        bill.Sqty = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mqty).ToArray()[0];

                        accsum.Errcode = "0000";
                        accsum.Errmsg = "成功";
                        accsum.Netamt = profilesItem.Netamt;
                        accsum.Fee = profilesItem.Fee;
                        accsum.Tax = profilesItem.Tax;
                        accsum.Mqty = profilesItem.Mqty;
                        accsum.Mamt = profilesItem.Mamt;
                    }
                    accsum.Sum = bill;
                    accsum.Profile = profiles;
                    #endregion

                }
                else
                {
                    #region
                    //歷史交易明細
                    foreach (HCMIOBean hCMIOBeanItem in hcmioBean)
                    {
                        Profile profile = new Profile();
                        profile.Bhno = hCMIOBeanItem.BHNO;
                        profile.Cseq = hCMIOBeanItem.CSEQ;
                        profile.Name = "";
                        profile.Stock = hCMIOBeanItem.STOCK;
                        profile.Stocknm = SingletonQueryProviderMSTMB.queryProvider.MSTMBQueryCNAME(hCMIOBeanItem.STOCK);
                        profile.Mdate = hCMIOBeanItem.Tdate;
                        profile.Dseq = hCMIOBeanItem.DSEQ;
                        profile.Dno = hCMIOBeanItem.DNO;
                        profile.Ttype = hCMIOBeanItem.TTYPE;
                        profile.Ttypename = TtypenameCalculate("HCMIO", hCMIOBeanItem.TTYPE, hCMIOBeanItem.BSTYPE, hCMIOBeanItem.ETYPE, hCMIOBeanItem.tTypeName);
                        profile.Bstype = hCMIOBeanItem.BSTYPE;
                        profile.Bstypename = BstypenameCalculate(hCMIOBeanItem.BSTYPE);
                        profile.Etype = hCMIOBeanItem.ETYPE;
                        profile.Mprice = hCMIOBeanItem.PRICE;
                        profile.Mqty = hCMIOBeanItem.QTY;
                        profile.Mamt = hCMIOBeanItem.AMT;
                        profile.Fee = Math.Floor(FeeCalculate(hCMIOBeanItem.QTY, hCMIOBeanItem.FEE));
                        profile.Tax = hCMIOBeanItem.TAX;
                        profile.Netamt = hCMIOBeanItem.NETAMT;
                        profiles.Add(profile);
                    }
                    //當日交易明細
                    foreach (TMHIOBean tMHIOBeanitem in tmhioBean)
                    {

                        if (TDATECalculate(DateTime.Now.ToString("yyyyMMdd"), root.Sdate, root.Edate))
                        {
                            Profile profile = new Profile();
                            profile.Bhno = tMHIOBeanitem.BHNO;
                            profile.Cseq = tMHIOBeanitem.CSEQ;
                            profile.Name = "";
                            profile.Stock = tMHIOBeanitem.STOCK;
                            profile.Stocknm = SingletonQueryProviderMSTMB.queryProvider.MSTMBQueryCNAME(tMHIOBeanitem.STOCK);
                            profile.Mdate = tMHIOBeanitem.Tdate;
                            profile.Dseq = tMHIOBeanitem.DSEQ;
                            profile.Dno = tMHIOBeanitem.JRNUM;
                            profile.Ttype = tMHIOBeanitem.TTYPE;
                            profile.Ttypename = TtypenameCalculate("TMHIO", tMHIOBeanitem.TTYPE, tMHIOBeanitem.BSTYPE, tMHIOBeanitem.ETYPE, "");
                            profile.Bstype = tMHIOBeanitem.BSTYPE;
                            profile.Bstypename = BstypenameCalculate(tMHIOBeanitem.BSTYPE);
                            profile.Etype = tMHIOBeanitem.ETYPE == "2" ? "1" : "0";
                            profile.Mprice = tMHIOBeanitem.PRICE;
                            profile.Mqty = tMHIOBeanitem.QTY;
                            profile.Mamt = Math.Floor(tMHIOBeanitem.PRICE * tMHIOBeanitem.QTY);
                            profile.Fee = Math.Floor(FeeCalculate(tMHIOBeanitem.QTY, tMHIOBeanitem.PRICE * 0.003m));
                            profile.Tax = Math.Floor(TaxCalculate(tMHIOBeanitem.BSTYPE, tMHIOBeanitem.PRICE, tMHIOBeanitem.QTY));
                            profile.Netamt = NetamtCalculate(tMHIOBeanitem.BSTYPE, profile.Mamt, profile.Fee, profile.Tax);
                            profiles.Add(profile);
                        }
                    }


                    var profilesLQ = from p in profiles
                                     group p by 0 into g
                                     select new
                                     {
                                         Mamt = g.Sum(g => g.Mamt),
                                         Fee = g.Sum(g => g.Fee),
                                         Tax = g.Sum(g => g.Tax),
                                         Netamt = g.Sum(g => g.Netamt),
                                         Mqty = g.Sum(g => g.Mqty)
                                     };
                    //https://dotblogs.com.tw/noncoder/2019/03/25/Lambda-GroupBy-Sum
                    var profilesMqtySumLQ = from p in profiles
                                            group p by new { p.Bstype } into g
                                            select new
                                            {
                                                Bstype = g.Key.Bstype,
                                                Mqty = g.Sum(g => g.Mqty),
                                                Mant = g.Sum(g => g.Mamt)
                                            };



                    if (profilesLQ.FirstOrDefault() == null)
                    {
                        AccsumErr accsumErr = new AccsumErr();
                        accsumErr.Errcode = "0008";
                        accsumErr.Errmsg = "查無符合設定條件之資料";
                        return accsumErr;
                    }
                    foreach (var profilesItem in profilesLQ)
                    {

                        bill.Cnbamt = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mant).ToArray()[0];
                        bill.Cnsamt = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mant).ToArray()[0];
                        bill.Cnfee = profilesItem.Fee;
                        bill.Cntax = profilesItem.Tax;
                        bill.Cnnetamt = profilesItem.Netamt;
                        bill.Bqty = (from a in profilesMqtySumLQ where a.Bstype == "B" select a.Mqty).ToArray()[0];
                        bill.Sqty = (from a in profilesMqtySumLQ where a.Bstype == "S" select a.Mqty).ToArray()[0];

                        accsum.Errcode = "0000";
                        accsum.Errmsg = "成功";
                        accsum.Netamt = profilesItem.Netamt;
                        accsum.Fee = profilesItem.Fee;
                        accsum.Tax = profilesItem.Tax;
                        accsum.Mqty = profilesItem.Mqty;
                        accsum.Mamt = profilesItem.Mamt;
                    }
                    accsum.Sum = bill;
                    accsum.Profile = profiles;
                    #endregion
                }

                return accsum;
            }
            catch (Exception ex)
            {
                Utils.Util.Log("交易異常: " + ex.ToString());
                AccsumErr accsumErr = new AccsumErr();
                accsumErr.Errcode = "9999";
                accsumErr.Errmsg = "交易異常: " + ex.Message;
                return accsumErr;
            }

        }
        #region
        //(5)	［手續費］請無條件捨去至整數；如為整股，最小手續費為20元；如為零股，最小手續費為1元。
        #endregion
        public decimal FeeCalculate(decimal QTY, decimal FEE)
        {
            if (QTY >= 1000)
            {
                if (FEE < 20)
                {
                    return 20;
                }
                return FEE;
            }
            else
            {
                if (FEE < 1)
                {
                    return 1;
                }
                return FEE;
            }
        }
        #region
        /*
         * (8)	［交易類別名稱］：
        如為[HCMIO]來源：
        	[TTYPE]為0且[BSTYPE]為B時，提供「現買」；
        	[TTYPE]為0且[BSTYPE]為B時，提供「現賣」；
        如為[TMHIO]來源：
        	[TTYPE]為0、[BSTYPE]為B且[ETYPE]為0時，提供「現買」；
        	[TTYPE]為0、[BSTYPE]為B且[ETYPE]為2時，提供「盤後零買」；
        	[TTYPE]為0、[BSTYPE]為B且[ETYPE]為5時，提供「盤中零買」；
        	[TTYPE]為0、[BSTYPE]為S且[ETYPE]為0時，提供「現賣」；
        	[TTYPE]為0、[BSTYPE]為S且[ETYPE]為2時，提供「盤後零賣」；
        	[TTYPE]為0、[BSTYPE]為S且[ETYPE]為5時，提供「盤中零賣」；
        */
        #endregion
        public string TtypenameCalculate(string table, string? TTYPE, string? BSTYPE, string? ETYPE, string tTypeName)
        {
            if (table == "HCMIO")
            {
                if (tTypeName != null || tTypeName != "")
                    return tTypeName;

                if (TTYPE == "0" && BSTYPE == "B")
                    return "現買";

                if (TTYPE == "0" && BSTYPE == "S")
                    return "現賣";
            }
            if (table == "TMHIO")
            {
                if (TTYPE == "0" && BSTYPE == "B" && ETYPE == "0")
                    return "現買";

                if (TTYPE == "0" && BSTYPE == "B" && ETYPE == "2")
                    return "盤後零買";

                if (TTYPE == "0" && BSTYPE == "B" && ETYPE == "5")
                    return "盤中零買";

                if (TTYPE == "0" && BSTYPE == "S" && ETYPE == "0")
                    return "現賣";

                if (TTYPE == "0" && BSTYPE == "S" && ETYPE == "2")
                    return "盤後零賣";

                if (TTYPE == "0" && BSTYPE == "S" && ETYPE == "5")
                    return "盤中零賣";

            }
            Utils.Util.Log("錯誤的交易類別名稱指標! \n 資料表:" + table + " TTYPE:" + TTYPE + " BSTYPE:" + BSTYPE + " ETYPE:" + ETYPE);
            return "";
        }
        public string BstypenameCalculate(string? BSTYPE)
        {
            if (BSTYPE == "B")
                return "買";
            if (BSTYPE == "S")
                return "賣";
            Utils.Util.Log("錯誤的買賣別名稱指標! \n BSTYPE:" + BSTYPE);
            return "";
        }
        public decimal TaxCalculate(string BSTYPE, decimal PRICE, decimal QTY)
        {
            try
            {
                if (BSTYPE == "B")
                    return 0;
                if (BSTYPE == "S")
                    return PRICE * QTY * 0.003m;
            }
            catch (Exception ex)
            {
                Utils.Util.Log(ex.ToString());
            }

            Utils.Util.Log("錯誤的買賣別名稱指標導致錯誤的交易稅計算指標! \n BSTYPE:" + BSTYPE + " PRICE:" + PRICE + "  QTY:" + QTY);
            return 0;
        }

        public decimal NetamtCalculate(string BSTYPE, decimal Mamt, decimal Fee, decimal Tax)
        {
            try
            {
                if (BSTYPE == "B")
                    return -(Mamt + Fee);
                if (BSTYPE == "S")
                    return (Mamt - Fee - Tax);
            }
            catch (Exception ex)
            {
                Utils.Util.Log(ex.ToString());
            }

            Utils.Util.Log("錯誤的買賣別名稱指標導致錯誤的淨收付計算指標! \n BSTYPE:" + BSTYPE + " Mamt:" + Mamt + "  Fee:" + Fee + "  Tax:" + Tax);
            return 0;
        }

        #region
        //(9)	如查詢起訖日期包含［當天日期］，則需查詢TMHIO
        #endregion

        public bool TDATECalculate(string DateNow, string Sdate, string Edate)
        {
            int DateNow1 = Convert.ToInt32(DateNow);
            int Sdate1 = Convert.ToInt32(Sdate);
            int Edate1 = Convert.ToInt32(Edate);

            if (Sdate1 <= DateNow1 && DateNow1 <= Edate1)
            {
                return true;
            }
            return false;
        }

        public (List<HCMIOBean>, List<HCMIOBean>, List<HCMIOBean>, List<HCMIOBean>) hCNTDInToStatement(List<HCNTDBean> hCNTDs, List<HCNRHBean> hCNRHs)
        {
            //List<HCMIOBean> hCMIOReturn = new List<HCMIOBean>();
            List<HCMIOBean> hCMIOBuysNow = new List<HCMIOBean>();
            List<HCMIOBean> hCMIOSalesNow = new List<HCMIOBean>();
            List<HCMIOBean> hCMIOSales = new List<HCMIOBean>();
            List<HCMIOBean> hCMIOBuys = new List<HCMIOBean>();
            foreach (var hC in hCNTDs)
            {
                HCMIOBean hCMIOBuy = new HCMIOBean();
                HCMIOBean hCMIOSale = new HCMIOBean();
                //  (買單)
                hCMIOBuy.Tdate = hC.TDATE;
                hCMIOBuy.BHNO = hC.BHNO;
                hCMIOBuy.CSEQ = hC.CSEQ;
                hCMIOBuy.DSEQ = hC.BDSEQ;
                hCMIOBuy.DNO = hC.BDNO;
                hCMIOBuy.WTYPE = "0";
                hCMIOBuy.STOCK = hC.STOCK;
                hCMIOBuy.TTYPE = "0";
                hCMIOBuy.ETYPE = "0";
                hCMIOBuy.BSTYPE = "B";
                hCMIOBuy.PRICE = hC.BPRICE;
                hCMIOBuy.QTY = hC.CQTY;
                //hCMIOBuy.AMT = ;
                hCMIOBuy.FEE = hC.BFEE;
                //hCMIOBuy.TAX = ;
                //hCMIOBuy.RVINT = ;
                hCMIOBuy.NETAMT = -1 * hC.COST;
                //hCMIOBuy.DBFEE = ;
                //hCMIOBuy.CRAMT = ;
                //hCMIOBuy.DNAMT = ;
                //hCMIOBuy.CRINT = ;
                //hCMIOBuy.DNINT = ;
                //hCMIOBuy.DLFEE = ;
                //hCMIOBuy.BFINT = ;
                //hCMIOBuy.OBAMT = ;
                //hCMIOBuy.INTAX = ;
                //hCMIOBuy.SFCODE = ;
                //hCMIOBuy.CDTDQTY = ;
                //hCMIOBuy.ORIGN = ;
                //hCMIOBuy.SALES = ;
                //hCMIOBuy.IOFLAG = ;
                //hCMIOBuy.ADJCOST = ;
                //hCMIOBuy.ADJDATE = ;
                //hCMIOBuy.STINTAX = ;
                //hCMIOBuy.HEALTHFEE = ;
                //hCMIOBuy.TRDATE = ;
                //hCMIOBuy.TRTIME = ;
                //hCMIOBuy.MODDATE = ;
                //hCMIOBuy.MODTIME = ;
                //hCMIOBuy.MODUSER = ;
                hCMIOBuy.tTypeName = "買沖";
                hCMIOBuysNow.Add(hCMIOBuy);

                //  (賣單)
                hCMIOSale.Tdate = hC.TDATE;
                hCMIOSale.BHNO = hC.BHNO;
                hCMIOSale.CSEQ = hC.CSEQ;
                hCMIOSale.DSEQ = hC.SDSEQ;
                hCMIOSale.DNO = hC.SDNO;
                hCMIOSale.WTYPE = "0";
                hCMIOSale.STOCK = hC.STOCK;
                hCMIOSale.TTYPE = "0";
                hCMIOSale.ETYPE = "0";
                hCMIOSale.BSTYPE = "S";
                hCMIOSale.PRICE = hC.SPRICE;
                hCMIOSale.QTY = hC.CQTY;
                hCMIOSale.AMT = hC.CQTY * hC.SPRICE;
                hCMIOSale.FEE = hC.SFEE;
                hCMIOSale.TAX = hC.TAX;
                //hCMIOSale.RVINT = ;
                hCMIOSale.NETAMT = hC.INCOME;
                //hCMIOSale.DBFEE = ;
                //hCMIOSale.CRAMT = ;
                //hCMIOSale.DNAMT = ;
                //hCMIOSale.CRINT = ;
                //hCMIOSale.DNINT = ;
                //hCMIOSale.DLFEE = ;
                //hCMIOSale.BFINT = ;
                //hCMIOSale.OBAMT = ;
                //hCMIOSale.INTAX = ;
                //hCMIOSale.SFCODE = ;
                //hCMIOSale.CDTDQTY = ;
                //hCMIOSale.ORIGN = ;
                //hCMIOSale.SALES = ;
                //hCMIOSale.IOFLAG = ;
                //hCMIOSale.ADJCOST = ;
                //hCMIOSale.ADJDATE = ;
                //hCMIOSale.STINTAX = ;
                //hCMIOSale.HEALTHFEE = ;
                //hCMIOSale.TRDATE = ;
                //hCMIOSale.TRTIME = ;
                //hCMIOSale.MODDATE = ;
                //hCMIOSale.MODTIME = ;
                //hCMIOSale.MODUSER = ;
                hCMIOSale.tTypeName = "賣沖";
                hCMIOSalesNow.Add(hCMIOSale);
            }

            foreach (var hc in hCNRHs)
            {
                HCMIOBean hCBuy = new HCMIOBean();
                HCMIOBean hCSale = new HCMIOBean();

                //  (買單)
                hCSale.Tdate = hc.TDATE;
                hCSale.BHNO = hc.BHNO;
                hCSale.CSEQ = hc.CSEQ;
                hCSale.DSEQ = hc.SDSEQ;
                hCSale.DNO = hc.SDNO;
                //hCSale.WTYPE = ;
                hCSale.STOCK = hc.STOCK;
                hCSale.TTYPE = "0";
                //hCSale.ETYPE = ;
                hCSale.BSTYPE = "B";
                hCSale.PRICE = hc.SPRICE;
                hCSale.QTY = hc.CQTY;
                //hCSale.AMT = ;
                hCSale.FEE = hc.SFEE;
                hCSale.TAX = hc.TAX;
                //hCBean.RVINT = ;
                hCSale.NETAMT = hc.INCOME;
                //hCBean.DBFEE = ;
                //hCBean.CRAMT = ;
                //hCBean.DNAMT = ;
                //hCBean.CRINT = ;
                //hCBean.DNINT = ;
                //hCBean.DLFEE = ;
                //hCBean.BFINT = ;
                //hCBean.OBAMT = ;
                //hCBean.INTAX = ;
                //hCBean.SFCODE = ;
                //hCBean.CDTDQTY = ;
                //hCBean.ORIGN = ;
                //hCBean.SALES = ;
                //hCBean.DATAFLAG = ;
                //hCBean.IOFLAG = ;
                //hCBean.ADJCOST = ;
                //hCBean.ADJDATE = ;
                //hCBean.STINTAX = ;
                //hCBean.HEALTHFEE = ;
                //hCBean.TRDATE = ;
                //hCBean.TRTIME = ;
                //hCBean.MODDATE = ;
                //hCBean.MODTIME = ;
                //hCBean.MODUSER = ;
                hCSale.tTypeName = "現買";
                hCMIOSales.Add(hCSale);

                //  (賣單)
                hCSale.Tdate = hc.TDATE;
                hCSale.BHNO = hc.BHNO;
                hCSale.CSEQ = hc.CSEQ;
                hCSale.DSEQ = hc.SDSEQ;
                hCSale.DNO = hc.SDNO;
                //hCBean.WTYPE =;
                hCSale.STOCK = hc.STOCK;
                hCSale.TTYPE = "0";
                //hCSale.ETYPE = ;
                hCSale.BSTYPE = "S";
                hCSale.PRICE = hc.SPRICE;
                hCSale.QTY = hc.CQTY;
                //hCBean.AMT = ;
                hCSale.FEE = hc.SFEE;
                hCSale.TAX = hc.TAX;
                //hCBean.RVINT = ;
                hCSale.NETAMT = hc.INCOME;
                //hCBean.DBFEE = ;
                //hCBean.CRAMT = ;
                //hCBean.DNAMT = ;
                //hCBean.CRINT = ;
                //hCBean.DNINT = ;
                //hCBean.DLFEE = ;
                //hCBean.BFINT = ;
                //hCBean.OBAMT = ;
                //hCBean.INTAX = ;
                //hCBean.SFCODE = ;
                //hCBean.CDTDQTY = ;
                //hCBean.ORIGN = ;
                //hCBean.SALES = ;
                //hCBean.DATAFLAG = ;
                //hCBean.IOFLAG = ;
                //hCBean.ADJCOST = ;
                //hCBean.ADJDATE = ;
                //hCBean.STINTAX = ;
                //hCBean.HEALTHFEE = ;
                //hCBean.TRDATE = ;
                //hCBean.TRTIME = ;
                //hCBean.MODDATE = ;
                //hCBean.MODTIME = ;
                //hCBean.MODUSER = ;
                hCSale.tTypeName = "現賣";
                hCMIOSales.Add(hCSale);

            }


            //不清楚再想甚麼

            //var joined = hCMIOSalesNow.Join(hCMIOSales,
            //    a => new { a.BHNO, a.CSEQ, a.DSEQ, a.DNO },
            //    b => new { b.BHNO, b.CSEQ, b.DSEQ, b.DNO },
            //    (a, b) => new { a.BHNO, a.CSEQ, a.DSEQ, a.DNO });
            //foreach (var item in joined)
            //{
            //    hCMIOSalesNow.Single(b => new { b.BHNO, b.CSEQ, b.DSEQ, b.DNO } == new { item.BHNO, item.CSEQ, item.DSEQ, item.DNO }).tTypeName = "現賣";//************************
            //}


            //如果一筆賣單同時沖銷多筆買單，將賣單合為同一張單

            var hCMIOSalesNowGroupIn = hCMIOSalesNow
                .Where(x => x.tTypeName == "賣沖")
                .GroupBy(x => new { x.Tdate, x.CSEQ, x.DNO, x.STOCK, x.BHNO, x.DSEQ })
                .Select(g => new HCMIOBean
                {
                    Tdate = g.Key.Tdate,
                    BHNO = g.Key.BHNO,
                    CSEQ = g.Key.CSEQ,
                    DSEQ = g.Key.DSEQ,
                    DNO = g.Key.DNO,
                    STOCK = g.Key.STOCK,
                    PRICE = g.First().PRICE,
                    AMT = g.Sum(x => x.AMT),
                    QTY = g.Sum(x => x.QTY),
                    FEE = g.Sum(x => x.FEE),
                    TAX = g.Sum(x => x.TAX),
                    NETAMT = g.Sum(x => x.NETAMT),
                    tTypeName = g.First().tTypeName
                });


            //return hCMIOBuysNow
            //    .Concat(hCMIOSalesNowGroupIn).ToList();
            return (hCMIOBuysNow, hCMIOSalesNow, hCMIOBuys, hCMIOSales);
        }
    }
}
