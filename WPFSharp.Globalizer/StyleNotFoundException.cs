using System;
using System.Runtime.Serialization;
using System.Security;

namespace WPFSharp.Globalizer
{
    public class StyleNotFoundException : ArgumentException, ISerializable
    {
        //
        // Summary:
        //     Initializes a new instance of the System.Globalization.StyleNotFoundException
        //     class with its message string set to a system-supplied message.
        public StyleNotFoundException() : base(DefaultMessage)
        {
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Globalization.StyleNotFoundException
        //     class with the specified error message.
        //
        // Parameters:
        //   message:
        //     The error message to display with this exception.
        public StyleNotFoundException(string message) : base(message)
        {
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Globalization.StyleNotFoundException
        //     class with a specified error message and the name of the parameter that is the
        //     cause this exception.
        //
        // Parameters:
        //   paramName:
        //     The name of the parameter that is the cause of the current exception.
        //
        //   message:
        //     The error message to display with this exception.
        public StyleNotFoundException(string paramName, string message) : base(message, paramName)
        {
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Globalization.StyleNotFoundException
        //     class with a specified error message and a reference to the inner exception that
        //     is the cause of this exception.
        //
        // Parameters:
        //   message:
        //     The error message to display with this exception.
        //
        //   innerException:
        //     The exception that is the cause of the current exception. If the innerException
        //     parameter is not a null reference, the current exception is raised in a catch
        //     block that handles the inner exception.
        public StyleNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Globalization.StyleNotFoundException
        //     class with a specified error message, the invalid Style Name, and the name
        //     of the parameter that is the cause this exception.
        //
        // Parameters:
        //   paramName:
        //     The name of the parameter that is the cause the current exception.
        //
        //   invalidStyleName:
        //     The Style Name that cannot be found.
        //
        //   message:
        //     The error message to display with this exception.
        public StyleNotFoundException(string paramName, string invalidStyleName, string message) : base(message, paramName)
        {
            InvalidStyleName = invalidStyleName;
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Globalization.StyleNotFoundException
        //     class with a specified error message, the invalid Style Name, and a reference
        //     to the inner exception that is the cause of this exception.
        //
        // Parameters:
        //   message:
        //     The error message to display with this exception.
        //
        //   invalidStyleName:
        //     The Style Name that cannot be found.
        //
        //   innerException:
        //     The exception that is the cause of the current exception. If the innerException
        //     parameter is not a null reference, the current exception is raised in a catch
        //     block that handles the inner exception.
        public StyleNotFoundException(string message, string invalidStyleName, Exception innerException) : base(message, innerException)
        {
            InvalidStyleName = invalidStyleName;
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Globalization.StyleNotFoundException
        //     class using the specified serialization data and context.
        //
        // Parameters:
        //   info:
        //     The object that holds the serialized object data.
        //
        //   context:
        //     The contextual information about the source or destination.
        [SecuritySafeCritical]
        protected StyleNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            InvalidStyleName = (string)info.GetValue("InvalidStyleName", typeof(string));
        }

        private static string DefaultMessage
        {
            get
            {
                return "Style is not supported.";
            }
        }

        //
        // Summary:
        //     Gets the Style Name that cannot be found.
        //
        // Returns:
        //     The invalid Style Name.
        public virtual string InvalidStyleName
        {
            get;
            private set;
        }

        //
        // Summary:
        //     Sets the System.Runtime.Serialization.SerializationInfo object with the parameter
        //     name and additional exception information.
        //
        // Parameters:
        //   info:
        //     The object that holds the serialized object data.
        //
        //   context:
        //     The contextual information about the source or destination.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     info is null.
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            base.GetObjectData(info, context);
            info.AddValue("InvalidStyleName", (object)this.InvalidStyleName, typeof(string));
        }
    }
}
