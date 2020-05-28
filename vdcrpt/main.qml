import QtQuick 2.5
import QtQuick.Controls 2.5
import QtQuick.Controls.Universal 2.5
import QtQuick.Layouts 1.1
import QtQuick.Dialogs 1.0

ApplicationWindow {
    width: 350
    height: 550

    minimumWidth: 450
    maximumWidth: 650
    minimumHeight: 600
    maximumHeight: 800

    visible: true
    title: "vdcrpt"

    Universal.theme: Universal.Dark

    property var version: "???"
    property var rendering: false
    property var statusText: "Ready!"
    property var fileSelected: false

    signal startCorrupting(
        inputVideo: url,
        chunkSize: int,
        stutterMin: int,
        stutterMax: int,
        iterations: int,
        outputVideo: url
    )

    RowLayout {
        anchors.fill: parent
        width: parent.width

        ColumnLayout {
            Layout.leftMargin: 24
            Layout.rightMargin: 24

            spacing: 20

            ColumnLayout {
                spacing: 8
                Label {
                    text: "vdcrpt"
                    topPadding: 0
                    font.pixelSize: 28
                }

                Label {
                    text: 'Version ' + version + ' - <a href="https://branchpanic.itch.io/vdcrpt">Instructions</a>'
                    onLinkActivated: Qt.openUrlExternally(link)
                    font.pixelSize: 14
                }
            }

            ColumnLayout {
                spacing: 4

                Label {
                    text: "Video"
                }

                RowLayout {
                    spacing: 12

                    Button {
                        id: fileButton
                        text: "Open"
                        onClicked: {
                            inputDialog.open()
                        }
                    }

                    Label {
                        id: fileField
                        text: "Select a video..."
                        Layout.fillWidth: true
                        elide: Text.ElideLeft
                    }
                }
            }

            ColumnLayout {
                spacing: 4

                Label {
                    text: "Chunk Size (bytes)"
                }

                SpinBox {
                    id: chunkSize
                    editable: true
                    from: 10
                    value: 1000
                    to: 100000
                    stepSize: 500
                    Layout.fillWidth: true
                }
            }
            
            ColumnLayout {
                spacing: 4
                GridLayout {
                    rows: 2
                    columns: 2
                    Layout.fillWidth: true

                    Label {
                        text: "Min Repetitions"
                    }

                    Label {
                        text: "Max Repetitions"
                    }

                    SpinBox {
                        id: minRepetitions
                        editable: true
                        from: 1
                        value: 10
                        to: 9999
                        stepSize: 10
                        Layout.fillWidth: true

                        onValueModified: {
                            if (minRepetitions.value > maxRepetitions.value) {
                                maxRepetitions.value = minRepetitions.value + 1
                            }
                        }
                    }

                    SpinBox {
                        id: maxRepetitions
                        editable: true
                        from: 2
                        value: 90
                        to: 10000
                        stepSize: 10
                        Layout.fillWidth: true

                        onValueModified: {
                            if (maxRepetitions.value < minRepetitions.value) {
                                minRepetitions.value = maxRepetitions.value - 1
                            }
                        }
                    }
                }
            }

            ColumnLayout {
                spacing: 4

                Label {
                    text: "Iterations"
                }

                SpinBox {
                    id: iterations
                    editable: true
                    from: 1
                    value: 15
                    to: 1000
                    stepSize: 5
                    Layout.fillWidth: true
                }
            }

            RowLayout {
                Button {
                    id: corruptButton
                    text: "Corrupt"
                    enabled: !rendering && fileSelected
                    Layout.fillWidth: true

                    onClicked: {
                        saveDialog.open()
                    }
                }
            }

            RowLayout {
                spacing: 12 

                BusyIndicator {
                    Layout.alignment: Qt.AlignHCenter
                    width: 80
                    height: 80
                    visible: rendering
                    running: rendering
                }
                
                Label {
                    Layout.fillWidth: true
                    text: statusText
                    wrapMode: Text.Wrap
                }
            }
        }
    }

    FileDialog {
        id: inputDialog
        title: "Choose Video"
        folder: shortcuts.home
        nameFilters: [ "Video files (*.avi *.mp4 *.webm *.gif)", "All files (*)" ]
        selectExisting: true

        onAccepted: {
            fileField.text = inputDialog.fileUrl.toString().replace('file:///', '')
            fileSelected = true
        }
    }

    FileDialog {
        id: saveDialog
        title: "Save Corrupted Video"
        folder: shortcuts.home
        nameFilters: [ "Video files (*.mp4)" ]
        selectExisting: false

        onAccepted: {
            rendering = true
            startCorrupting(inputDialog.fileUrl, chunkSize.value, minRepetitions.value, maxRepetitions.value, iterations.value, saveDialog.fileUrl)
        }
    }
}
