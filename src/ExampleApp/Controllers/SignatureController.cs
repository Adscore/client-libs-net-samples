using AdScore.Signature;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExampleApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignatureController : ControllerBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signature">contains result.signature from the Adscore Javascript API</param>
        /// <returns></returns>
        [HttpGet("Verify")]
        public IActionResult Verify([FromQuery] string signature)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var adscoreKey = "KEY_GOES_HERE";

            List<string> ipAddresses = GetIpAddresses();

            var verifyResult = SignatureVerifier.Verify(signature, userAgent, "customer", adscoreKey, ipAddresses.ToArray());

            // Possible scores and verdicts:
            // score  verdict
            // 0     "ok"
            // 3     "junk"
            // 6     "proxy"
            // 9     "bot"
            // null  "no verdict" (verification is not possible. Check error for more details)
            var score = verifyResult.Score;
            var verdict = verifyResult.Verdict;

            // Error message explaining e.g why "bot" score has been assigned to given request
            var error = verifyResult.Error;

            // Ip addresses used to check against the signature. It can be either IPv4 or IPv6
            var ipAddress = verifyResult.IpAddress;

            // Epoch millis when request has been executed, resolved from the signature
            var requestTime = verifyResult.RequestTime;

            // Epoch millis when signature for given request has been generated
            var signatureTime = verifyResult.SignatureTime;

            // True if either requestTime or signatureTime expired
            var expired = verifyResult.Expired;

            return Ok(String.Format("Score:{0}, verdict:{1}, expired:{2}, error:{3}, ipAddress:{4}, requestTime:{5}, signatureTime:{6}",
                score, verdict, expired, error, ipAddress, requestTime, signatureTime));
        }

        /// <summary>
        /// Crude example how to get ip addresses
        /// </summary>
        /// <returns></returns>
        private List<string> GetIpAddresses()
        {
            var ipAddresses = new List<string>();

            var forwardedHeaders = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').ToList();

            if (forwardedHeaders != null)
            {
                ipAddresses.AddRange(forwardedHeaders);
            }

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (remoteIpAddress != null)
            {
                ipAddresses.Add(remoteIpAddress);
            }

            return ipAddresses;
        }
    }
}
