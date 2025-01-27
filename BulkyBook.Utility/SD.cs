﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Utility
{
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        public const string Role_User_Company = "Company Customer";
        public const string Role_User_Indi = "individual Customer";

        public const string ssShoppingCart = "Shopping Cart Session";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";


        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedpayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";

        public static double GetPriceBasedOnQunatity(double qunatity, double price, double price50, double price100)
        {
            if (qunatity < 50)
            {
                return price;
            }
            else
            {
                if (qunatity < 100)
                {
                    return price50;
                }
                else
                {
                    return price100;
                }
            }
        }

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}
