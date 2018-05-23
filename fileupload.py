# Copyright 2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file
# except in compliance with the License. A copy of the License is located at
#
#     http://aws.amazon.com/apache2.0/
#
# or in the "license" file accompanying this file. This file is distributed on an "AS IS"
# BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
# License for the specific language governing permissions and limitations under the License.
"""
A BitBucket Builds template for deploying an application revision to AWS CodeDeploy
narshiva@amazon.com
v1.0.0
"""
from __future__ import print_function
import sys
import argparse
import boto3
import glob
import os
from botocore.exceptions import ClientError

def upload_to_s3(bucket, artifact, bucket_key):
    """
    Uploads an artifact to Amazon S3
    """
    print("Uploading file named " + artifact + " to " + bucket_key)
    try:
        client = boto3.client('s3')
    except ClientError as err:
        print("Failed to create boto3 client.\n" + str(err))
        return False
    try:
        client.put_object(
            Body=open(artifact, 'rb'),
            Bucket=bucket,
            Key=bucket_key
        )
    except ClientError as err:
        print("Failed to upload artifact to S3.\n" + str(err))
        return False
    except IOError as err:
        print("Failed to access artifact in this directory.\n" + str(err))
        return False
    return True


def main():

    parser = argparse.ArgumentParser()
    parser.add_argument("bucket", help="Name of the existing S3 bucket")
    parser.add_argument("prefix", help="Name of the prefix folder to upload too")
    args = parser.parse_args()
    for file in glob.glob("*.gamedef"):
        if not upload_to_s3(args.bucket, file, args.prefix+ "/" + file):
            sys.exit(1)
    for file in glob.glob("*.iss"):
        if not upload_to_s3(args.bucket, file, args.prefix+ "/" + file):
            sys.exit(1)

if __name__ == "__main__":
    main()