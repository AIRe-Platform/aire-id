using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Aire.Id.Oauth2.Models
{
	[Serializable]
	public class OauthException : Exception
	{
        // Required
		public OauthError Error { get; private set; } = OauthError.ServerError;

        // Optional
        public string ErrorUri { get; set; }

        // Required if state was present in the authorization request
        public string State { get; set; }

        // Set to use redirection
        public string Redirect { get; set; }

		public OauthException(OauthError error)
			: base("")
		{
			Error = error;
		}

		public OauthException(OauthError error, string description)
			: base(description)
		{
			Error = error;
		}

		public IActionResult OauthErrorResult()
		{
			var error = new OauthErrorResponse {
                Error = Error,
                ErrorUri = ErrorUri,
                ErrorDescription = Message,
                State = State
            };

            if(Redirect != null)
            {
                var uri = new Uri(QueryHelpers.AddQueryString(Redirect, error.ToDictionary()));   
                return new RedirectResult(uri.AbsoluteUri);
            }
			{
                var status = Error switch
                {
                    OauthError.AccessDenied => HttpStatusCode.Forbidden,
                    OauthError.UnauthorizedClient => HttpStatusCode.Unauthorized,
                    OauthError.ServerError => HttpStatusCode.InternalServerError,
                    OauthError.TemporarilyUnavailable => HttpStatusCode.ServiceUnavailable,
                    _ => HttpStatusCode.BadRequest,
                };

                return new ObjectResult(error) { StatusCode = (int) status };
			}
		}
	}
}
