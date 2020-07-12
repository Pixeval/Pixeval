package io.pixeval.statistics

import io.ktor.client.HttpClient
import io.ktor.client.engine.cio.CIO
import io.ktor.client.features.json.GsonSerializer
import io.ktor.client.features.json.JsonFeature
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.util.KtorExperimentalAPI
import java.net.HttpURLConnection

object GithubTracker {
    @KtorExperimentalAPI
    suspend fun getGithubReleaseTotalDownloads(): Long {
        val client = HttpClient(CIO) {
            install(JsonFeature) {
                serializer = GsonSerializer()
            }
        }
        val entity = client.get<GithubReleases>("https://api.github.com/repos/Rinacm/Pixeval/releases") {
            header("Accept", "application/vnd.github.v3+json")
        }
        return entity.map { it.assets[0].downloadCount }.reduce { acc, l -> acc + l }
    }
}