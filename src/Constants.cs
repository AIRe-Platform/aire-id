// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


namespace Aire.Id;

public static class AireConstants
{
    public static class Queues
    {
        public const string Mail = "mail-queue";
    }

    public const int MaxVerificationRetryCount = 10;
    public const int MinPasswordLength = 8;
    public const int MaxUserFirstNameAndLastNameLength = 50;
    public const int MaxUserBioLength = 2000;
}
