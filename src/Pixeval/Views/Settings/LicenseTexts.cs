// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Views.Settings;

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
}
