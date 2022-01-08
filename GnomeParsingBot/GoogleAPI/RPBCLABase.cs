using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.GoogleAPI
{
    public abstract class RPBCLABase
    {
        protected static object _credsLockObj = new object();
        protected UserCredential _creds;

        public RPBCLABase(UserCredential creds)
        {
            this._creds = creds;
        }

        protected BaseClientService.Initializer CreateServiceInitializer()
        {
            BaseClientService.Initializer init = null;

            lock (_credsLockObj)
            {
                if (_creds == null)
                    throw new Exception("_creds cannot be null");

                _creds.RefreshTokenAsync(CancellationToken.None).Wait();

                init = new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _creds,
                    ApplicationName = "GnomeParsing - RPB and CLA",
                };
            }

            return init;
        }
    }
}
