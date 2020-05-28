package io.pixeval.statistics

import io.ktor.application.call
import io.ktor.application.install
import io.ktor.features.CORS
import io.ktor.features.ForwardedHeaderSupport
import io.ktor.http.content.files
import io.ktor.http.content.static
import io.ktor.http.content.staticRootFolder
import io.ktor.response.respondRedirect
import io.ktor.response.respondText
import io.ktor.routing.get
import io.ktor.routing.routing
import io.ktor.server.engine.embeddedServer
import io.ktor.server.netty.Netty
import io.ktor.util.KtorExperimentalAPI
import io.pixeval.statistics.helper.Properties
import java.io.File
import java.util.*

@KtorExperimentalAPI
fun main(args: Array<String>) {
    val props = Properties.read(args[0])
    embeddedServer(Netty) {
        install(ForwardedHeaderSupport)
        install(CORS) {
            anyHost()
        }
        routing {
            static("/") {
                staticRootFolder = File(props["pixeval.static.location"] ?: error("cannot find property pixeval.static.location"))
                files("pixeval")
            }
            get("/statistics") {
                call.respondText((GithubTracker.getGithubReleaseTotalDownloads() + LocalTracker.getLocal()).toString())
            }
            get("/Pixeval.zip") {
                LocalTracker.increment()
                call.respondRedirect(props["pixeval.download.redirect"] ?: error("cannot find property pixeval.download.redirect"))
            }
            get("/Pixeval/version.txt") {
                call.respondText(File(props["pixeval.version.location"] ?: error("cannot find property pixeval.version.location")).readText())
            }
            get("Pixeval/checksum.txt") {
                call.respondText(File(props["pixeval.checksum.location"] ?: error("cannot find property pixeval.checksum.location")).readText())
            }
        }
    }.start(true)
}