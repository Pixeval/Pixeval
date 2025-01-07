// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Pages.Misc;

// ReSharper disable InconsistentNaming
public static class LicenseTexts
{
    public static string MIT(string year, string name) =>
        $"""
         The MIT License (MIT)
         <br/>

         Copyright (c) {year} {name}
         <br/>

         Permission is hereby granted, free of charge, to any person obtaining a copy
         of this software and associated documentation files (the "Software"), to deal
         in the Software without restriction, including without limitation the rights
         to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
         copies of the Software, and to permit persons to whom the Software is
         furnished to do so, subject to the following conditions:
         <br/>
         The above copyright notice and this permission notice shall be included in all
         copies or substantial portions of the Software.
         <br/>
         THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
         IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
         FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
         AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
         LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
         OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
         SOFTWARE.
         """;

    public static string BSD3(string year, string name) =>
        $"""
         BSD 3-Clause License
         <br/>
         Copyright (c) {year}, {name}
         All rights reserved.
         <br/>
         Redistribution and use in source and binary forms, with or without
         modification, are permitted provided that the following conditions are met:
         <br/>
         1. Redistributions of source code must retain the above copyright notice, this
            list of conditions and the following disclaimer.
         <br/>
         2. Redistributions in binary form must reproduce the above copyright notice,
            this list of conditions and the following disclaimer in the documentation
            and/or other materials provided with the distribution.
         <br/>
         3. Neither the name of the copyright holder nor the names of its
            contributors may be used to endorse or promote products derived from
            this software without specific prior written permission.
         <br/>
         THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
         AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
         IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
         DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
         FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
         DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
         SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
         CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
         OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
         OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
         """;

    public const string ImageSharp =
        """
        Six Labors Split License
        Version 1.0, June 2022
        Copyright (c) Six Labors
        <br/>
        TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION
        <br/>
        1. Definitions.
        <br/>
           "You" (or "Your") shall mean an individual or Legal Entity exercising permissions granted by this License.
        <br/>
           "Source" form shall mean the preferred form for making modifications, including but not limited to software source
            code, documentation source, and configuration files.
        <br/>
           "Object" form shall mean any form resulting from mechanical transformation or translation of a Source form, including
            but not limited to compiled object code, generated documentation, and conversions to other media types.
        <br/>
           "Work" (or "Works") shall mean any Six Labors software made available under the License, as indicated by a
           copyright notice that is included in or attached to the work.
        <br/>
           "Direct Package Dependency" shall mean any Work in Source or Object form that is installed directly by You.
        <br/>
           "Transitive Package Dependency" shall mean any Work in Object form that is installed indirectly by a third party
            dependency unrelated to Six Labors.
        <br/>
        2. License
        <br/>
           Works in Source or Object form are split licensed and may be licensed under the Apache License, Version 2.0 or a
           Six Labors Commercial Use License.
        <br/>
           Licenses are granted based upon You meeting the qualified criteria as stated. Once granted,
           You must reference the granted license only in all documentation.
        <br/>
           Works in Source or Object form are licensed to You under the Apache License, Version 2.0 if.
        <br/>
           - You are consuming the Work in for use in software licensed under an Open Source or Source Available license.
           - You are consuming the Work as a Transitive Package Dependency.
           - You are consuming the Work as a Direct Package Dependency in the capacity of a For-profit company/individual with
             less than 1M USD annual gross revenue.
           - You are consuming the Work as a Direct Package Dependency in the capacity of a Non-profit organization
             or Registered Charity.
        <br/>
           For all other scenarios, Works in Source or Object form are licensed to You under the Six Labors Commercial License
           which may be purchased by visiting https://sixlabors.com/pricing/.
        """;

    public const string QuestPDF =
        """
        ﻿# QuestPDF License
        <br/>
        ## License Selection Guide
        <br/>
        Welcome to QuestPDF! This guide will help you understand how to select the appropriate license for our library, based on your usage context.
        <br/>
        The licensing options for QuestPDF include the MIT license (which is free), and two tiers of paid licenses: the Professional License and the Enterprise License.
        <br/>
        ### License Equality
        <br/>
        We believe in offering the full power of QuestPDF to all our users, regardless of the license they choose. Whether you're operating under our Community MIT, Professional, or Enterprise licenses, you can enjoy the same comprehensive range of features:
        <br/>
        - Full access to all QuestPDF features.
        - Support for commercial usage.
        - Freedom to create and deploy unlimited closed-source projects, applications, and APIs.
        - Royalty-free redistribution of the compiled library with your applications.
          Transitive Dependency Usage
          If you're using QuestPDF as a transitive dependency, you're free to use it under the MIT license without any cost. However, you're welcomed and encouraged to support the project by purchasing a paid license if you find the library valuable.
        <br/>
        ### Non-profit Usage
        <br/>
        If you represent an open-source project, a charitable organization, or are using QuestPDF for educational purposes or training courses, you can also use QuestPDF for free under the MIT license.
        <br/>
        ### Small Businesses
        <br/>
        For companies generating less than $1M USD in annual gross revenue, you can use QuestPDF under the MIT license for free.
        <br/>
        ### Larger Businesses
        <br/>
        Companies with an annual gross revenue exceeding $1M USD are required to purchase a paid license. The type of license you need depends on the number of developers working on projects that use QuestPDF:
        <br/>
        Professional License - If there are up to 10 developers in your company who are using QuestPDF, you need to purchase the Professional License.
        <br/>
        Enterprise License - If your company has more than 10 developers using QuestPDF, the Enterprise License is the right choice.
        <br/>
        ### Beyond Compliance
        <br/>
        Remember, purchasing a license isn't just about adhering to our guidelines, but also supporting the development of QuestPDF. Your contribution helps us to improve the library and offer top-notch support to all users.

        <br/>
        <br/>

        ## QuestPDF Community MIT License
        <br/>
        ### License Permissions
        <br/>
        Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
        <br/>
        ### Copyright
        <br/>
        The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
        <br/>
        ### Limitation Of Liability
        <br/>
        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

        <br/><br/>

        ## QuestPDF Professional and Enterprise Use License
        <br/>
        ### Do No Harm
        <br/>
        By downloading or using the Software, the Licensee agrees not to utilize the software in a manner which is disparaging to QuestPDF, and not to rent, lease or otherwise transfer rights to the Software.
        <br/>
        ### License Permissions
        <br/>
        Grants the use of the Software by a specified number of developers to create and deploy closed-source software for unlimited end user organizations ("The Organization") in multiple locations. This license covers unlimited applications or projects. The Software may be deployed upon any number of machines for the end-use of The Organization. This license also intrinsically covers for development, staging and production servers for each project.
        <br/>
        Grants the right to distribute the Software (without royalty) as part of packaged commercial products.
        <br/>
        ### License Fees
        <br/>
        A. If you wish to use the Software in a production environment, the purchase of a license is required. This license is perpetual, granting you continued use of the Software in accordance with the terms and conditions of this Agreement. The cost of the license is as indicated on the pricing page.
        <br/>
        B. Upon purchasing a license, you are also enrolled in a yearly, recurring subscription for software updates. This subscription is valid for a period of one year from the date of purchase, and it will automatically renew each year unless cancelled. We recommend maintaining your subscription as long as you are performing active software development to ensure you have access to the latest updates and improvements to the Software.
        <br/>
        C. However, it should be noted that the perpetual license allows use of only the latest library revision available at the time of or within the active subscription period, in accordance with the terms and conditions of this Agreement.
        <br/>
        D. If you wish to use the Software in a non-production environment, such as for testing and evaluation purposes, you may download and access the source and/or binaries at no charge. This access is subject to all license limitations and restrictions set forth in this Agreement.
        <br/>
        ### Ownership
        <br/>
        QuestPDF shall at all times retain ownership of the QuestPDF Software library and all subsequent copies.
        <br/>
        ### Copyright
        <br/>
        Title, ownership rights, and intellectual property rights in and to the Software shall remain with QuestPDF. The Software is protected by the international copyright laws. Title, ownership rights, and intellectual property rights in and to the content accessed through the Software is the property of the applicable content owner and may be protected by applicable copyright or other law. This License gives you no rights to such content.
        <br/>
        ### Limitation Of Liability
        <br/>
        THIS SOFTWARE IS PROVIDED "AS IS," WITHOUT A WARRANTY OF ANY KIND. ALL EXPRESS OR IMPLIED CONDITIONS, REPRESENTATIONS AND WARRANTIES, INCLUDING ANY IMPLIED WARRANTY OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NON-INFRINGEMENT, ARE HEREBY EXCLUDED. QUESTPDF AND ITS LICENSORS SHALL NOT BE LIABLE FOR ANY DAMAGES SUFFERED BY LICENSEE AS A RESULT OF USING, MODIFYING OR DISTRIBUTING THE SOFTWARE OR ITS DERIVATIVES. IN NO EVENT WILL QUESTPDF OR ITS LICENSORS BE LIABLE FOR ANY LOST REVENUE, PROFIT OR DATA, OR FOR DIRECT, INDIRECT, SPECIAL, CONSEQUENTIAL, INCIDENTAL OR PUNITIVE DAMAGES, HOWEVER CAUSED AND REGARDLESS OF THE THEORY OF LIABILITY, ARISING OUT OF THE USE OF OR INABILITY TO USE SOFTWARE, EVEN IF QUESTPDF HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
        """;
}
