package io.pixeval.statistics

import java.io.File
import java.nio.file.Paths

object LocalTracker {
    private val location = "${System.getenv("LOCALAPPDATA")}\\pixeval.statistics\\counter.txt"

    fun increment() {
        val file = File(location)
        if (file.exists()) {
            file.writeText("${getLocal() + 1}")
        } else {
            File(Paths.get(location).parent.toString()).mkdirs()
            file.createNewFile()
            file.writeText("1")
        }
    }

    fun getLocal(): Long {
        val file = File(location)
        return if (file.exists()) {
            file.readText().toLong()
        } else 0
    }
}