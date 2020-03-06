using BancoDados;
using System;



namespace padvMVC.Negocio
{
    public abstract class Negocio
    {
        protected IBanco conexao = null;

        public Negocio()
        {
            conexao = ConfigDB.GetInstanciaCon();
        }
        public IBanco GetConnection()
        {
            return conexao;
        }
        public void SetConnection(IBanco conexao)
        {
            this.conexao = conexao;
        }
        public int? ErrorCode()
        {
            return conexao.errorCode();
        }

        public string GetFormatDateForDB(DateTime? date, bool isFinish = false, bool WithHour = false)
        {
            DateTime newDate;

            if (date == null)

            {
                return null;
            }

            if (!isFinish)
            {
                if (WithHour)
                {
                    newDate = date.Value;
                }
                else
                {
                    newDate = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 00, 00, 00);
                }
            }
            else
            {
                newDate = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59);
            }
            return newDate.ToString("yyyy-MM-dd HH:mm:ss");

        }
        
    }
}