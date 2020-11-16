using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public int TransferTypeId { get; set; }
        public int TransferStatusId { get; set; }
        public int AccountFrom { get; set; }
        public int AccountTo { get; set; }
        public decimal Amount { get; set; }
    }
    public class TransferTypes
    {
        public int TransferTypeId { get; set; }
        public string TransferTypeDesc { get; set; }
        public TransferTypes(int transferTypeId)
        {
            TransferTypeId = transferTypeId;
            if (TransferTypeId == 1)
            {
                TransferTypeDesc = "Request";
            }
            else if (TransferTypeId == 2)
            {
                TransferTypeDesc = "Send";
            }

        }

    }
    public class TransferStatuses
    {
        public int TransferStatusId { get; set; } = 2;
        public string TransferStatusDesc { get; set; }
        public TransferStatuses(int transferStatusId)
        {
            TransferStatusId = transferStatusId;
            if (transferStatusId == 1)
            {
                TransferStatusDesc = "Pending";
            }
            else if (transferStatusId == 2)
            {
                TransferStatusDesc = "Approved";
            }
            else if (transferStatusId == 3)
            {
                TransferStatusDesc = "Rejected";
            }
        }
    }
}
