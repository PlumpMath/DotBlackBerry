﻿using System;
using System.Runtime.InteropServices;

namespace BlackBerry.BPS.Dialog
{
    /// <summary>
    /// The format of dates and times in notification dialogs.
    /// </summary>
    [AvailableSince(10, 0)]
    public enum DateTimeFormat : int
    {
        /// <summary>
        /// Specify that a short format be used to display the date and time.
        /// </summary>
        [AvailableSince(10, 0)]
        Short = 0,
        /// <summary>
        /// Specify that a medium format be used to display the date and time.
        /// </summary>
        [AvailableSince(10, 0)]
        Medium = 1,
        /// <summary>
        /// Specify that a long format be used to display the date and time.
        /// </summary>
        [AvailableSince(10, 0)]
        Long = 2,
        /// <summary>
        /// Specify that the date and time not be displayed.
        /// </summary>
        [AvailableSince(10, 0)]
        None = 3
    }

    /// <summary>
    /// Notification dialog.
    /// </summary>
    [AvailableSince(10, 0)]
    public sealed class NotificationDialog : Dialog
    {
        #region PInvoke

        [DllImport("bps")]
        private static extern int dialog_create_notification(out IntPtr dialog);

        [DllImport("bps")]
        private static extern int dialog_set_notification_message_text(IntPtr dialog, [MarshalAs(UnmanagedType.LPStr)]string text);

        [DllImport("bps")]
        private static extern int dialog_set_notification_message_has_emoticons(IntPtr dialog, bool has_emoticons);

        [DllImport("bps")]
        private static extern int dialog_set_notification_subject_text(IntPtr dialog, [MarshalAs(UnmanagedType.LPStr)]string text);

        [DllImport("bps")]
        private static extern int dialog_set_notification_icon(IntPtr dialog, [MarshalAs(UnmanagedType.LPStr)]string path);

        [DllImport("bps")]
        private static extern int dialog_set_notification_start_date_time(IntPtr dialog, long time);

        [DllImport("bps")]
        private static extern int dialog_set_notification_start_date_format(IntPtr dialog, int date_format, int time_format, int week_format);

        [DllImport("bps")]
        private static extern int dialog_set_notification_start_date_format_elapsed(IntPtr dialog, int elapsed_format);

        [DllImport("bps")]
        private static extern int dialog_set_notification_end_date_time(IntPtr dialog, long time);

        [DllImport("bps")]
        private static extern int dialog_set_notification_end_date_format(IntPtr dialog, int date_format, int time_format, int week_format);

        [DllImport("bps")]
        private static extern int dialog_set_notification_end_date_format_elapsed(IntPtr dialog, int elapsed_format);

        [DllImport("bps")]
        private static extern int dialog_set_notification_content_locked(IntPtr dialog, bool locked);

        [DllImport("bps")]
        private static extern int dialog_set_notification_content_selectable(IntPtr dialog, bool selectable);

        #endregion

        /// <summary>
        /// Create a new notification dialog.
        /// </summary>
        [AvailableSince(10, 0)]
        public NotificationDialog()
            : base()
        {
        }

        internal override void CreateDialog()
        {
            if (dialog_create_notification(out handle) != BPS.BPS_SUCCESS)
            {
                Util.ThrowExceptionForLastErrno();
            }
        }

        #region Properties

        /// <summary>
        /// Set the message text of a notification dialog.
        /// </summary>
        [AvailableSince(10, 0)]
        public string Message
        {
            [AvailableSince(10, 0)]
            set
            {
                if (dialog_set_notification_message_text(handle, value) != BPS.BPS_SUCCESS)
                {
                    Util.ThrowExceptionForLastErrno();
                }
                UpdateDialog();
            }
        }

        /// <summary>
        /// Set whether the message text has emoticons.
        /// </summary>
        [AvailableSince(10, 0)]
        public bool HasEmoticons
        {
            [AvailableSince(10, 0)]
            set
            {
                if (dialog_set_notification_message_has_emoticons(handle, value) != BPS.BPS_SUCCESS)
                {
                    Util.ThrowExceptionForLastErrno();
                }
                UpdateDialog();
            }
        }

        /// <summary>
        /// Set the subject text of a notification dialog.
        /// </summary>
        [AvailableSince(10, 0)]
        public string Subject
        {
            [AvailableSince(10, 0)]
            set
            {
                if (dialog_set_notification_subject_text(handle, value) != BPS.BPS_SUCCESS)
                {
                    Util.ThrowExceptionForLastErrno();
                }
                UpdateDialog();
            }
        }

        /// <summary>
        /// Set the icon of a progress dialog.
        /// </summary>
        [AvailableSince(10, 0)]
        public string IconPath
        {
            [AvailableSince(10, 0)]
            set
            {
                if (dialog_set_notification_icon(handle, value) != BPS.BPS_SUCCESS)
                {
                    Util.ThrowExceptionForLastErrno();
                }
                UpdateDialog();
            }
        }

        /// <summary>
        /// Set whether the content is locked in a notification dialog.
        /// </summary>
        [AvailableSince(10, 0)]
        public bool ContentLocked
        {
            [AvailableSince(10, 0)]
            set
            {
                if (dialog_set_notification_content_locked(handle, value) != BPS.BPS_SUCCESS)
                {
                    Util.ThrowExceptionForLastErrno();
                }
                UpdateDialog();
            }
        }

        /// <summary>
        /// Set whether the content is selectable in a notification dialog.
        /// </summary>
        [AvailableSince(10, 0)]
        public bool ContentSelectable
        {
            [AvailableSince(10, 0)]
            set
            {
                if (dialog_set_notification_content_selectable(handle, value) != BPS.BPS_SUCCESS)
                {
                    Util.ThrowExceptionForLastErrno();
                }
                UpdateDialog();
            }
        }

        #endregion

        #region Functions

        private static DateTime GetEpoch()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Set the start date/time of a notification dialog.
        /// </summary>
        /// <param name="dateTime">The start date/time.</param>
        /// <param name="date">The format of the date portion of the start date/time.</param>
        /// <param name="time">The format of the time portion of the start date/time.</param>
        /// <param name="week">The format of the week portion of the start date/time.</param>
        /// <returns>true on success, false on error.</returns>
        [AvailableSince(10, 0)]
        public bool SetStartDateTime(DateTime dateTime, DateTimeFormat date, DateTimeFormat time, DateTimeFormat week = DateTimeFormat.None)
        {
            if (dialog_set_notification_start_date_format(handle, (int)date, (int)time, (int)week) != BPS.BPS_SUCCESS && 
                dialog_set_notification_start_date_time(handle, (long)(dateTime.ToUniversalTime() - GetEpoch()).TotalMilliseconds) != BPS.BPS_SUCCESS)
            {
                return false;
            }
            return UpdateDialog(false);
        }

        /// <summary>
        /// Set the start elapsed time of a notification dialog.
        /// </summary>
        /// <param name="timeSpan">The start elapsed time.</param>
        /// <param name="format">The format of the relative start elapsed time.</param>
        /// <returns>true on success, false on error.</returns>
        [AvailableSince(10, 0)]
        public bool SetStartElapsedTime(TimeSpan timeSpan, DateTimeFormat format)
        {
            if (dialog_set_notification_start_date_format_elapsed(handle, (int)format) != BPS.BPS_SUCCESS && 
                dialog_set_notification_start_date_time(handle, (long)timeSpan.TotalMilliseconds) != BPS.BPS_SUCCESS)
            {
                return false;
            }
            return UpdateDialog(false);
        }

        /// <summary>
        /// Set the end date/time of a notification dialog.
        /// </summary>
        /// <param name="dateTime">The end date/time.</param>
        /// <param name="date">The format of the date portion of the end date/time.</param>
        /// <param name="time">The format of the time portion of the end date/time.</param>
        /// <param name="week">The format of the week portion of the end date/time.</param>
        /// <returns>true on success, false on error.</returns>
        [AvailableSince(10, 0)]
        public bool SetEndDateTime(DateTime dateTime, DateTimeFormat date, DateTimeFormat time, DateTimeFormat week = DateTimeFormat.None)
        {
            if (dialog_set_notification_end_date_format(handle, (int)date, (int)time, (int)week) != BPS.BPS_SUCCESS &&
                dialog_set_notification_end_date_time(handle, (long)(dateTime.ToUniversalTime() - GetEpoch()).TotalMilliseconds) != BPS.BPS_SUCCESS)
            {
                return false;
            }
            return UpdateDialog(false);
        }

        /// <summary>
        /// Set the end elapsed time of a notification dialog.
        /// </summary>
        /// <param name="timeSpan">The end elapsed time.</param>
        /// <param name="format">The format of the relative end elapsed time.</param>
        /// <returns>true on success, false on error.</returns>
        [AvailableSince(10, 0)]
        public bool SetEndElapsedTime(TimeSpan timeSpan, DateTimeFormat format)
        {
            if (dialog_set_notification_end_date_format_elapsed(handle, (int)format) != BPS.BPS_SUCCESS &&
                dialog_set_notification_end_date_time(handle, (long)timeSpan.TotalMilliseconds) != BPS.BPS_SUCCESS)
            {
                return false;
            }
            return UpdateDialog(false);
        }

        #endregion
    }
}
